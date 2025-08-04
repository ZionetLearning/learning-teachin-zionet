using Engine.Constants;
using Engine.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Engine.Services;

public sealed class ChatAiService : IChatAiService
{
    private readonly Kernel _kernel;
    private readonly ILogger<ChatAiService> _log;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions;
    private readonly IChatCompletionService _chat;
    private readonly ISystemPromptProvider _prompt;

    public ChatAiService(
        Kernel kernel,
        ILogger<ChatAiService> log,
        IMemoryCache cache,
        MemoryCacheEntryOptions cacheOptions,
        ISystemPromptProvider prompt)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _cacheOptions = cacheOptions;
        _chat = _kernel.GetRequiredService<IChatCompletionService>();
        _prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
    }

    public async Task<AiResponseModel> ProcessAsync(AiRequestModel request, CancellationToken ct = default)
    {
        _log.LogInformation("AI processing request {Id}", request.Id);

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now > request.SentAt + request.TtlSeconds)
        {
            _log.LogWarning("Request {Id} is expired. Skipping.", request.Id);
            return new AiResponseModel
            {
                Id = request.Id,
                ThreadId = request.ThreadId,
                Status = "expired",
                Error = "TTL expired"
            };
        }

        try
        {
            var historyKey = CacheKeys.ChatHistory(request.ThreadId);
            var history = _cache.GetOrCreate(historyKey, _ => new ChatHistory()) ?? new ChatHistory();

            if (history.Count == 0)
            {
                history.AddSystemMessage(_prompt.Prompt);
            }

            history.AddUserMessage(request.Question);

            var settings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var result = await _chat.GetChatMessageContentAsync(
                history,
                executionSettings: settings,
                kernel: _kernel, //todo: add tools
                cancellationToken: ct);

            var answer = result.Content ?? string.Empty;

            history.AddAssistantMessage(answer);

            _cache.Set(historyKey, history, _cacheOptions);

            return new AiResponseModel
            {
                Id = request.Id,
                ThreadId = request.ThreadId,
                Answer = answer
            };

        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error while processing AI request {Id}", request.Id);
            return new AiResponseModel
            {
                Id = request.Id,
                ThreadId = request.ThreadId,
                Status = "error",
                Error = ex.Message
            };
        }
    }
}