using System.Text.Json;
using System.Text.Json.Nodes;
using Engine.Constants;
using Engine.Models.Chat;
using Engine.Services.Clients.AccessorClient.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Polly;

namespace Engine.Services;

public sealed class ChatAiService : IChatAiService
{
    private readonly Kernel _kernel;
    private readonly ILogger<ChatAiService> _log;
    private readonly IChatCompletionService _chat;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IAsyncPolicy<ChatMessageContent> _kernelPolicy;

    public ChatAiService(
        Kernel kernel,
        ILogger<ChatAiService> log,
        IMemoryCache cache,
        IOptions<MemoryCacheEntryOptions> cacheOptions,
        IRetryPolicy retryPolicy)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        _chat = _kernel.GetRequiredService<IChatCompletionService>();
        _kernelPolicy = _retryPolicy.CreateKernelPolicy(_log);
    }

    public async Task<ChatAiServiceResponse> ChatHandlerAsync(ChatAiServiseRequest request, CancellationToken ct = default)
    {
        _log.LogInformation("AI processing thread {ThreadId}", request.ThreadId);

        var response = new ChatAiServiceResponse
        {
            RequestId = request.RequestId,
            ThreadId = request.ThreadId

        };

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now > request.SentAt + request.TtlSeconds)
        {
            _log.LogWarning("Request: {RequestId} is expired Skipping.", request.RequestId);

            response.Status = ChatAnswerStatus.Expired;
            response.Error = "TTL expired";

            return response;
        }

        try
        {
            // 1) Построить SK ChatHistory из сырого JSON
            var skHistory = BuildSkHistoryFromRaw(request.History);

            // Если история пустая — добавим системный промпт
            if (skHistory.Count == 0)
            {
                var prompt = Prompts.Combine(Prompts.SystemDefault, Prompts.DetailedExplanation);
                skHistory.AddSystemMessage(prompt);
            }

            // 2) Добавить юзерское сообщение в SK-историю
            var cleanUserMsg = request.UserMessage.Trim();
            skHistory.AddUserMessage(cleanUserMsg);

            var settings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            // 3) Вызов LLM
            var result = await _kernelPolicy.ExecuteAsync(
                async ct2 => await _chat.GetChatMessageContentAsync(skHistory, settings, _kernel, ct2),
                ct);

            if (string.IsNullOrWhiteSpace(result?.Content))
            {
                response.Status = ChatAnswerStatus.Fail;
                response.Error = "Empty model result";
                return response;
            }

            // 4) Добавить ассистента в SK-историю
            skHistory.Add(result);

            // 5) Сформировать обновлённый сырой JSON (append user + assistant)
            var updatedRaw = AppendMessagesToRaw(request.History,
                ("user", cleanUserMsg),
                ("assistant", result.Content!));

            response.Status = ChatAnswerStatus.Ok;
            response.Answer = new ChatMessage
            {
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Role = MessageRole.Assistant,
                Content = result.Content!
            };
            response.UpdatedHistory = updatedRaw;
            return response;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error while processing request: {RequestId}", request.RequestId);
            response.Status = ChatAnswerStatus.Fail;
            response.Error = ex.Message;
            return response;
        }
    }

    private static ChatHistory BuildSkHistory(IEnumerable<ChatMessage> db)
    {
        var history = new ChatHistory();

        var systemPrompt = Prompts.Combine(
            Prompts.SystemDefault,
            Prompts.DetailedExplanation
        );
        history.AddSystemMessage(systemPrompt);

        foreach (var m in db)
        {
            switch (m.Role)
            {
                case MessageRole.User:
                    history.AddUserMessage(m.Content);
                    break;
                case MessageRole.Assistant:
                    history.AddAssistantMessage(m.Content);
                    break;
                case MessageRole.System:
                    history.AddSystemMessage(m.Content);
                    break;
                default:
                    history.AddUserMessage(m.Content);
                    break;
            }
        }

        return history;
    }

    private static ChatHistory BuildSkHistoryFromRaw(JsonElement raw)
    {
        var history = new ChatHistory();

        JsonArray? msgs = null;
        try
        {
            if (raw.ValueKind is not JsonValueKind.Undefined and not JsonValueKind.Null)
            {
                var node = JsonNode.Parse(raw.GetRawText()) as JsonObject;
                msgs = node?["messages"] as JsonArray;
            }
        }
        catch
        {
            // если format неожиданно другой — начнём с чистого списка + системный промпт добавим выше
        }

        if (msgs is not null)
        {
            foreach (var item in msgs)
            {
                var obj = item as JsonObject;
                var role = obj?["role"]?.GetValue<string>();
                var content = obj?["content"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                switch (role)
                {
                    case "system":
                        history.AddSystemMessage(content);
                        break;
                    case "assistant":
                        history.AddAssistantMessage(content);
                        break;
                    case "user":
                        history.AddUserMessage(content);
                        break;
                    default:
                        history.AddUserMessage(content);
                        break;
                }
            }
        }

        return history;
    }
    private static JsonElement AppendMessagesToRaw(JsonElement raw, params (string role, string content)[] toAppend)
    {
        JsonObject root;
        JsonArray messages;

        if (raw.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            root = new JsonObject();
            messages = new JsonArray();
            root["messages"] = messages;
        }
        else
        {
            root = (JsonNode.Parse(raw.GetRawText()) as JsonObject) ?? new JsonObject();
            messages = root["messages"] as JsonArray ?? new JsonArray();
            root["messages"] = messages;
        }

        foreach (var (role, content) in toAppend)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            messages.Add(new JsonObject
            {
                ["role"] = role,
                ["content"] = content
            });
        }

        using var doc = JsonDocument.Parse(root.ToJsonString());
        // Clone, чтобы отвязаться от doc
        return doc.RootElement.Clone();
    }
}