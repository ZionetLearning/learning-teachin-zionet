using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Azure.AI.OpenAI;
using Engine.Constants.Chat;
using Engine.Models;
using Engine.Models.Chat;
using Engine.Services.Clients.AccessorClient;
using Engine.Services.Clients.AccessorClient.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ChatMessage = Engine.Services.Clients.AccessorClient.Models.ChatMessage;
using TextContent = Microsoft.Extensions.AI.TextContent;

namespace Engine.Services;

public sealed class ChatAiService : IChatAiService
{
    private readonly ILogger<ChatAiService> _log;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IChatClient _chatClient;
    private readonly AzureOpenAIClient _azureClient;
    private readonly AzureOpenAiSettings _cfg;
    private readonly IList<AITool> _tools;
    private readonly IAccessorClient _accessorClient;

    public ChatAiService(
        AzureOpenAIClient azureClient,
        IOptions<AzureOpenAiSettings> options,
        ILogger<ChatAiService> log,
        IRetryPolicy retryPolicy,
        IList<AITool> tools,
        IAccessorClient accessorClient)
    {
        _azureClient = azureClient ?? throw new ArgumentNullException(nameof(azureClient));
        _cfg = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        _tools = tools ?? throw new ArgumentNullException(nameof(tools));
        _accessorClient = accessorClient ?? throw new ArgumentNullException(nameof(accessorClient));

        _chatClient = _azureClient.GetChatClient(_cfg.DeploymentName).AsIChatClient();
    }

    public async Task<ChatAiServiceResponse> ChatHandlerAsync(ChatAiServiceRequest request, CancellationToken ct = default)
    {
        _log.LogInformation("ChatAI request started {RequestId} for User {UserId}, Thread {ThreadId}",
            request.RequestId, request.UserId, request.ThreadId);

        var resp = new ChatAiServiceResponse
        {
            RequestId = request.RequestId,
            ThreadId = request.ThreadId
        };

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now > request.SentAt + request.TtlSeconds)
        {
            resp.Status = ChatAnswerStatus.Expired;
            resp.Error = "TTL expired";
            return resp;
        }

        try
        {
            var isNewThread = !(request.AgentThreadState is { History.ValueKind: not JsonValueKind.Undefined and not JsonValueKind.Null });
            string? system = null;

            if (isNewThread)
            {
                system = await ResolveSystemIfNeededAsync(ct);
            }

            var agent = _chatClient.CreateAIAgent(
                instructions: isNewThread ? system : null,
                name: "MainChatAgent",
                tools: _tools);

            var thread = isNewThread
                ? agent.GetNewThread()
                : agent.DeserializeThread(NormalizeTypeFirst(request.AgentThreadState!.History));

            var runOptions = new ChatClientAgentRunOptions(new ChatOptions { Temperature = 0.2f });

            var ar = await agent.RunAsync(request.UserMessage.ToString(), thread, runOptions, ct);
            var text = ar.Text?.Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                resp.Status = ChatAnswerStatus.Fail;
                resp.Error = "Empty model result";
                return resp;
            }

            var threadJson = thread.Serialize().GetRawText();

            resp.Status = ChatAnswerStatus.Ok;
            resp.Answer = new ChatMessage
            {
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Role = MessageRole.Assistant,
                Content = text
            };
            resp.ThreadStoreJson = threadJson;
            return resp;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "ChatAI failed {RequestId}", request.RequestId);
            resp.Status = ChatAnswerStatus.Fail;
            resp.Error = ex.Message;
            return resp;
        }
    }

    public async IAsyncEnumerable<ChatAiStreamDelta> ChatStreamAsync(
        ChatAiServiceRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        _log.LogInformation("ChatAI stream started {RequestId} for User {UserId}, Thread {ThreadId}",
            request.RequestId, request.UserId, request.ThreadId);

        // TTL
        var now = DateTimeOffset.UtcNow;
        if (now.ToUnixTimeSeconds() > request.SentAt + request.TtlSeconds)
        {
            yield return new ChatAiStreamDelta
            {
                RequestId = request.RequestId,
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Stage = ChatStreamStage.Expired,
                IsFinal = true
            };
            yield break;
        }

        var isNewThread = !(request.AgentThreadState is { History.ValueKind: not JsonValueKind.Undefined and not JsonValueKind.Null });
        string? system = null;
        if (isNewThread)
        {
            system = await ResolveSystemIfNeededAsync(ct);
        }

        var agent = _chatClient.CreateAIAgent(
               name: "MainChatAgent",
               instructions: isNewThread ? system : null,
               tools: _tools);

        var thread = isNewThread
            ? agent.GetNewThread()
            : agent.DeserializeThread(NormalizeTypeFirst(request.AgentThreadState!.History));

        var messages = new List<Microsoft.Extensions.AI.ChatMessage>();

        if (isNewThread && !string.IsNullOrWhiteSpace(system))
        {
            messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.System, system) { CreatedAt = now });
        }

        messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, request.UserMessage) { CreatedAt = now });

        var runOptions = new ChatClientAgentRunOptions(new ChatOptions { Temperature = 0.2f });

        var sb = new StringBuilder();

        await foreach (var update in agent.RunStreamingAsync(messages, thread, runOptions, ct))
        {
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            if (update.Contents is null)
            {
                continue;
            }

            foreach (var c in update.Contents)
            {
                switch (c)
                {
                    case TextContent t when !string.IsNullOrEmpty(t.Text):
                        sb.Append(t.Text);
                        yield return new ChatAiStreamDelta
                        {
                            RequestId = request.RequestId,
                            ThreadId = request.ThreadId,
                            UserId = request.UserId,
                            Delta = t.Text,
                            Stage = ChatStreamStage.Model,
                            IsFinal = false
                        };
                        break;

                    case FunctionCallContent fc:
                        yield return new ChatAiStreamDelta
                        {
                            RequestId = request.RequestId,
                            ThreadId = request.ThreadId,
                            UserId = request.UserId,
                            ToolCall = fc.Name,
                            Stage = ChatStreamStage.Tool,
                            IsFinal = false
                        };
                        break;

                    case FunctionResultContent fr:
                        yield return new ChatAiStreamDelta
                        {
                            RequestId = request.RequestId,
                            ThreadId = request.ThreadId,
                            UserId = request.UserId,
                            ToolResult = fr?.Result?.ToString(),
                            Stage = ChatStreamStage.ToolResult,
                            IsFinal = false
                        };
                        break;
                }
            }
        }

        var finalJson = thread.Serialize().GetRawText();

        yield return new ChatAiStreamDelta
        {
            RequestId = request.RequestId,
            ThreadId = request.ThreadId,
            UserId = request.UserId,
            Delta = sb.ToString(),
            Stage = ChatStreamStage.Final,
            IsFinal = true,
            ThreadStoreJson = finalJson
        };
    }

    // ---- helpers -- //delete after migration

    private async Task<string?> ResolveSystemIfNeededAsync(CancellationToken ct)
    {
        try
        {
            var p = await _accessorClient.GetPromptAsync(PromptsKeys.SystemDefault, ct);
            var s = p?.Content?.Trim();
            if (!string.IsNullOrWhiteSpace(s))
            {
                return s;
            }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Failed to fetch system prompt, proceeding without it.");
        }

        return "You are a helpful assistant. Keep answers concise.";
    }

    private static JsonElement NormalizeTypeFirst(JsonElement root)
    {
        using var ms = new MemoryStream();
        using (var w = new Utf8JsonWriter(ms))
        {
            WriteWithTypeFirst(root, w);
        }

        using var doc = JsonDocument.Parse(ms.ToArray());
        return doc.RootElement.Clone();
    }

    private static void WriteWithTypeFirst(JsonElement el, Utf8JsonWriter w)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                w.WriteStartObject();

                // Сначала пишем $type (если есть)
                if (el.TryGetProperty("$type", out var typeVal))
                {
                    w.WritePropertyName("$type");
                    WriteWithTypeFirst(typeVal, w);
                }

                // Затем остальные свойства, кроме $type
                foreach (var prop in el.EnumerateObject())
                {
                    if (prop.NameEquals("$type"))
                    {
                        continue;
                    }

                    w.WritePropertyName(prop.Name);
                    WriteWithTypeFirst(prop.Value, w);
                }

                w.WriteEndObject();
                break;

            case JsonValueKind.Array:
                w.WriteStartArray();
                foreach (var item in el.EnumerateArray())
                {
                    WriteWithTypeFirst(item, w);
                }

                w.WriteEndArray();
                break;

            case JsonValueKind.String:
                w.WriteStringValue(el.GetString());
                break;

            case JsonValueKind.Number:
                // сохраняем точное значение (целое/вещественное) через сырые байты
                w.WriteRawValue(el.GetRawText());
                break;

            case JsonValueKind.True:
                w.WriteBooleanValue(true);
                break;

            case JsonValueKind.False:
                w.WriteBooleanValue(false);
                break;

            case JsonValueKind.Null:
                w.WriteNullValue();
                break;
        }
    }

    // temporary bridge: SK → AF ChatMessage (current turn only)
    private static List<Microsoft.Extensions.AI.ChatMessage> MapLegacyHistoryToMessages(
        Microsoft.SemanticKernel.ChatCompletion.ChatHistory? legacy)
    {
        if (legacy is null || legacy.Count == 0)
        {
            return [];
        }

        var list = new List<Microsoft.Extensions.AI.ChatMessage>(legacy.Count);
        foreach (var m in legacy)
        {
            if (string.IsNullOrWhiteSpace(m.Content))
            {
                continue;
            }

            if (m.Role == Microsoft.SemanticKernel.ChatCompletion.AuthorRole.System)
            {
                continue;
            }

            var role = m.Role == Microsoft.SemanticKernel.ChatCompletion.AuthorRole.Assistant
                ? ChatRole.Assistant
                : ChatRole.User;

            list.Add(new Microsoft.Extensions.AI.ChatMessage(role, m.Content!));
        }

        return list;
    }
}
