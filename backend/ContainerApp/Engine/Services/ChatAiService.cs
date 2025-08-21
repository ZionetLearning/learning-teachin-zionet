using System.Text.Json;
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
            var skHistory = BuildSkHistoryFromRaw(request.History);

            if (skHistory.Count == 0)
            {
                var SystemPrompt = Prompts.Combine(Prompts.SystemDefault, Prompts.DetailedExplanation);
                skHistory.AddSystemMessage(SystemPrompt);
            }

            var baseline = skHistory.Count;

            var cleanUserMsg = request.UserMessage.Trim();
            skHistory.AddUserMessage(cleanUserMsg);

            var settings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var result = await _kernelPolicy.ExecuteAsync(
                async ct2 => await _chat.GetChatMessageContentAsync(skHistory, settings, _kernel, ct2),
                ct);

            if (string.IsNullOrWhiteSpace(result?.Content))
            {
                response.Status = ChatAnswerStatus.Fail;
                response.Error = "Empty model result";
                return response;
            }

            skHistory.Add(result);

            HistoryEnvelope envelope;

            if (request.History.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                envelope = new HistoryEnvelope();
            }
            else
            {
                try
                {
                    envelope = System.Text.Json.JsonSerializer.Deserialize<HistoryEnvelope>(request.History.GetRawText(), HistoryJsonOptions)
                               ?? new HistoryEnvelope();
                }
                catch
                {
                    envelope = new HistoryEnvelope();
                }
            }

            if (!EnvelopeHasSystem(envelope))
            {
                var SystemPrompt = Prompts.Combine(Prompts.SystemDefault, Prompts.DetailedExplanation);
                envelope.Messages.Insert(0, new ChatMessageContent(AuthorRole.System, SystemPrompt));
            }

            for (var i = baseline; i < skHistory.Count; i++)
            {
                envelope.Messages.Add(skHistory[i]);
            }

            var json = System.Text.Json.JsonSerializer.Serialize(envelope, HistoryJsonOptions);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var updatedRaw = doc.RootElement.Clone();

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

    private static ChatHistory BuildSkHistoryFromRaw(JsonElement raw)
    {
        var history = new ChatHistory();

        try
        {
            if (raw.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                return history;
            }

            var env = System.Text.Json.JsonSerializer.Deserialize<HistoryEnvelope>(raw.GetRawText(), HistoryJsonOptions);
            if (env?.Messages is not null && env.Messages.Count > 0)
            {
                history.AddRange(env.Messages);
            }
        }
        catch
        {
            //todo: action if exception
        }

        return history;
    }

    private static readonly JsonSerializerOptions HistoryJsonOptions = CreateHistoryJsonOptions();

    private static JsonSerializerOptions CreateHistoryJsonOptions()
    {
        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        opts.Converters.Add(new Helpers.ChatMessageContentConverter());
        return opts;
    }

    private static bool EnvelopeHasSystem(HistoryEnvelope env)
    {
        foreach (var m in env.Messages)
        {
            if (m.Role == AuthorRole.System)
            {
                return true;
            }
        }

        return false;
    }
}