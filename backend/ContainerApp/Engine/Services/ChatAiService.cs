using Engine.Constants;
using Engine.Messaging;
using Engine.Models;
using Engine.Services.Clients;
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
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions;
    private readonly IChatCompletionService _chat;
    private readonly IRetryPolicyProvider _retryPolicyProvider;
    private readonly IAsyncPolicy<ChatMessageContent> _kernelPolicy;
    private readonly IAccessorClient _accessorClient;

    public ChatAiService(
        Kernel kernel,
        ILogger<ChatAiService> log,
        IMemoryCache cache,
        IOptions<MemoryCacheEntryOptions> cacheOptions,
        IRetryPolicyProvider retryPolicyProvider,
        IAccessorClient accessorClient)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _cacheOptions = cacheOptions?.Value ?? throw new ArgumentNullException(nameof(cacheOptions));
        _retryPolicyProvider = retryPolicyProvider ?? throw new ArgumentNullException(nameof(retryPolicyProvider));
        _chat = _kernel.GetRequiredService<IChatCompletionService>();
        _kernelPolicy = _retryPolicyProvider.CreateKernelPolicy(_log);
        _accessorClient = accessorClient ?? throw new ArgumentNullException(nameof(accessorClient));
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
            //log the history key for debugging
            _log.LogDebug("Using chat history key: {HistoryKey}", historyKey);
            var cachedHistory = _cache.GetOrCreate(historyKey, _ => new ChatHistory()) ?? new ChatHistory();
            //log the cached history count for debugging
            _log.LogDebug("Cached history count: {Count}", cachedHistory.Count);
            //log the history messages for debugging
            _log.LogDebug("Cached history messages: {Messages}", cachedHistory);
            if (cachedHistory.Count == 0)
            {
                _log.LogInformation("No cached history found for thread {ThreadId}. Fetching from accessor.", request.ThreadId);
                var prompt = Prompts.Combine(
                    Prompts.SystemDefault,
                    Prompts.DetailedExplanation
                );
                cachedHistory.AddSystemMessage(prompt);
                var dbHistory = await _accessorClient.GetChatHistoryAsync(request.ThreadId);

                if (dbHistory?.Messages != null)
                {
                    foreach (var message in dbHistory.Messages)
                    {
                        switch (message.Role)
                        {
                            case "user":
                                cachedHistory.AddUserMessage(message.Message);
                                break;
                            case "assistant":
                                cachedHistory.AddAssistantMessage(message.Message);
                                break;

                        }
                    }
                }
            }

            _log.LogDebug("Updated cached history after accessor fetch: {Messages}", cachedHistory);
            cachedHistory.AddUserMessage(request.Question);

            var settings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var result = await _kernelPolicy
                .ExecuteAsync(async ct2 => await _chat.GetChatMessageContentAsync(
                    cachedHistory,
                    executionSettings: settings,
                    kernel: _kernel,
                    cancellationToken: ct2), ct);

            var answer = result.Content ?? string.Empty;

            _log.LogDebug("AI response generated: {Answer}", answer);
            cachedHistory.AddAssistantMessage(answer);

            _cache.Set(historyKey, cachedHistory, _cacheOptions);

            //call the accessor client to store the chat messages
            var storeRequest = new StoreChatMessagesRequest
            (
                ThreadId: request.ThreadId,
                UserMessage: request.Question,
                AssistantMessage: answer
            );

            var storeResult = await _accessorClient.StoreChatMessagesAsync(storeRequest);
            _log.LogDebug("Stored chat messages for thread {ThreadId}: {Success}", request.ThreadId, storeResult);
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