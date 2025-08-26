using System.Text.Json;
using Engine.Constants;
using Engine.Models.Chat;
using Engine.Services.Clients.AccessorClient.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Polly;

namespace Engine.Services;

public sealed class ChatAiService : IChatAiService
{
    private readonly Kernel _kernel;
    private readonly ILogger<ChatAiService> _log;
    private readonly IChatTitleService _chatTitleService;
    private readonly IChatCompletionService _chat;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IAsyncPolicy<ChatMessageContent> _kernelPolicy;

    public ChatAiService(
        Kernel kernel,
        ILogger<ChatAiService> log,
        IMemoryCache cache,
        IChatTitleService chatTitleService,
        IOptions<MemoryCacheEntryOptions> cacheOptions,
        IRetryPolicy retryPolicy)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _chatTitleService = chatTitleService ?? throw new ArgumentNullException(nameof(chatTitleService));
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
            var skHistory = ToChatHistoryFromElement(request.History);

            var storyForKernel = CloneToChatHistory(skHistory);

            var baseline = storyForKernel.Count;

            if (storyForKernel.Count == 0)
            {
                var SystemPrompt = Prompts.Combine(Prompts.SystemDefault, Prompts.DetailedExplanation);
                storyForKernel.AddSystemMessage(SystemPrompt);
            }

            var cleanUserMsg = request.UserMessage.Trim();
            storyForKernel.AddUserMessage(cleanUserMsg);

            var settings = new AzureOpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var result = await _kernelPolicy.ExecuteAsync(
                async ct2 => await _chat.GetChatMessageContentAsync(storyForKernel, settings, _kernel, ct2),
                ct);

            if (string.IsNullOrWhiteSpace(result?.Content))
            {
                response.Status = ChatAnswerStatus.Fail;
                response.Error = "Empty model result";
                return response;
            }

            storyForKernel.Add(result);

            AppendDelta(skHistory, storyForKernel, baseline);

            var name = request.Name;

            if (name == "New chat")
            {
                try
                {
                    name = await _chatTitleService.GenerateAsync(skHistory, ct);

                }
                catch (Exception exName)
                {
                    _log.LogError(exName, "Error while processing naming chat: {RequestId}", request.RequestId);
                    name = DateTime.UtcNow.ToString("HHmm_dd_MM");

                }
            }

            var envelope = new HistoryEnvelope { Messages = skHistory.ToList() };
            var updatedRaw = ToJsonElementCompat(envelope);

            response.Status = ChatAnswerStatus.Ok;
            response.Answer = new ChatMessage
            {
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Role = MessageRole.Assistant,
                Content = result.Content!
            };
            response.Name = name;
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

    private static JsonElement ToJsonElementCompat<T>(T value)
    {
        return System.Text.Json.JsonSerializer.SerializeToElement(value!);

    }

    private static ChatHistory CloneToChatHistory(ChatHistory source)
    {
        var h = new ChatHistory();
        foreach (var m in source)
        {
            h.Add(DeepCloneMessage(m));
        }

        return h;
    }

    private static ChatMessageContent DeepCloneMessage(ChatMessageContent msg)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(msg);
        var clone = System.Text.Json.JsonSerializer.Deserialize<ChatMessageContent>(json);

        return clone!;
    }

    private static void AppendDelta(ChatHistory baseHistory, ChatHistory srcWithNew, int baseline)
    {
        for (var i = baseline; i < srcWithNew.Count; i++)
        {
            baseHistory.Add(DeepCloneMessage(srcWithNew[i]));
        }
    }

    private static ChatHistory ToChatHistoryFromElement(JsonElement element)
    {
        var history = new ChatHistory();

        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return history;
        }

        if (!element.TryGetProperty("messages", out var messages) || messages.ValueKind != JsonValueKind.Array)
        {
            return history;
        }

        foreach (var msgEl in messages.EnumerateArray())
        {
            var role = AuthorRole.User;
            if (msgEl.TryGetProperty("Role", out var roleObj) && roleObj.ValueKind == JsonValueKind.Object)
            {
                var label = roleObj.GetPropertyOrNull("Label")?.GetString();
                role = MapRole(label);
            }
            else
            {
                var label = msgEl.GetPropertyOrNull("role")?.GetString();
                if (!string.IsNullOrWhiteSpace(label))
                {
                    role = MapRole(label);
                }
            }

            var itemsCol = new ChatMessageContentItemCollection();

            if (msgEl.TryGetProperty("Items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var it in itemsEl.EnumerateArray())
                {
                    var typeStr = it.GetPropertyOrNull("$type")?.GetString();

                    switch (typeStr)
                    {
                        case "TextContent":
                        {
                            var text = it.GetPropertyOrNull("Text")?.GetString();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                itemsCol.Add(new TextContent(text));
                            }

                            break;
                        }

                        case "FunctionCallContent":
                        {
                            var fn = it.GetPropertyOrNull("FunctionName")?.GetString();
                            if (string.IsNullOrWhiteSpace(fn))
                            {
                                break;
                            }

                            var plugin = it.GetPropertyOrNull("PluginName")?.GetString();
                            var id = it.GetPropertyOrNull("Id")?.GetString();
                            var argsKA = ElementToKernelArguments(it.GetPropertyOrNull("Arguments"));

                            itemsCol.Add(new FunctionCallContent(fn!, plugin, id, argsKA));
                            break;
                        }

                        case "FunctionResultContent":
                        {
                            var fn = it.GetPropertyOrNull("FunctionName")?.GetString() ?? string.Empty;
                            var plugin = it.GetPropertyOrNull("PluginName")?.GetString();
                            var callId = it.GetPropertyOrNull("CallId")?.GetString();
                            object? resultObj = null;
                            var resEl = it.GetPropertyOrNull("Result");
                            if (resEl.HasValue)
                            {
                                resultObj = ElementToObj(resEl.Value);
                            }

                            itemsCol.Add(new FunctionResultContent(fn, plugin, callId, resultObj));
                            break;
                        }

                        default:
                        {
                            var text = it.GetPropertyOrNull("Text")?.GetString();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                itemsCol.Add(new TextContent(text));
                            }

                            break;
                        }
                    }
                }
            }

            var content = msgEl.GetPropertyOrNull("content")?.GetString() ?? string.Empty;

            var modelId = msgEl.GetPropertyOrNull("ModelId")?.GetString()
                              ?? msgEl.GetPropertyOrNull("modelId")?.GetString();

            var metadata = ReadMetadataFromMessage(msgEl);

            ChatMessageContent message;
            if (itemsCol.Count > 0)
            {
                message = new ChatMessageContent(role, itemsCol, null, null, null, metadata);
            }
            else
            {
                message = new ChatMessageContent(role, content, null, null, null, metadata);
            }

            if (!string.IsNullOrWhiteSpace(modelId))
            {
                message.ModelId = modelId;
            }

            history.Add(message);
        }

        return history;
    }

    private static AuthorRole MapRole(string? label)
    {
        switch (label?.Trim().ToLowerInvariant())
        {
            case "system":
                return AuthorRole.System;
            case "assistant":
                return AuthorRole.Assistant;
            case "developer":
                return AuthorRole.Developer;
            case "tool":
                return AuthorRole.Tool;
            case "user":
            default:
                return AuthorRole.User;
        }
    }

    private static KernelArguments? ElementToKernelArguments(JsonElement? elNullable)
    {
        if (!elNullable.HasValue)
        {
            return null;
        }

        var el = elNullable.Value;
        if (el.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var ka = new KernelArguments();
        foreach (var p in el.EnumerateObject())
        {
            ka[p.Name] = ElementToObj(p.Value);
        }

        return ka;
    }

    private static object? ElementToObj(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.String:
                return el.GetString();

            case JsonValueKind.Number:
            {
                if (el.TryGetInt64(out var l))
                {
                    return l;
                }

                if (el.TryGetDouble(out var d))
                {
                    return d;
                }

                return el.GetRawText();
            }

            case JsonValueKind.True:
            case JsonValueKind.False:
                return el.GetBoolean();

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;

            case JsonValueKind.Array:
            {
                var list = new List<object?>();
                foreach (var item in el.EnumerateArray())
                {
                    list.Add(ElementToObj(item));
                }

                return list;
            }

            case JsonValueKind.Object:
            {
                var dict = new Dictionary<string, object?>();
                foreach (var p in el.EnumerateObject())
                {
                    dict[p.Name] = ElementToObj(p.Value);
                }

                return dict;
            }

            default:
                return el.GetRawText();
        }
    }

    private static IReadOnlyDictionary<string, object?>? ReadMetadataFromMessage(JsonElement msgEl)
    {
        if (msgEl.TryGetProperty("Metadata", out var metaEl) && metaEl.ValueKind == JsonValueKind.Object)
        {
            return (IReadOnlyDictionary<string, object?>)ElementToObj(metaEl)!;
        }

        if (msgEl.TryGetProperty("metadata", out metaEl) && metaEl.ValueKind == JsonValueKind.Object)
        {
            return (IReadOnlyDictionary<string, object?>)ElementToObj(metaEl)!;
        }

        return null;
    }
}

file static class JsonElementExt
{
    public static JsonElement? GetPropertyOrNull(this JsonElement element, string name)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var v))
        {
            return v;
        }

        return null;
    }
}