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

            response.Status = "expired";
            response.Error = "TTL expired";

            return response;
        }

        try
        {
            var history = BuildSkHistory(request.History);

            if (history.Count == 0)
            {
                var prompt = Prompts.Combine(
                     Prompts.SystemDefault,
                     Prompts.DetailedExplanation
                     );

                history.AddSystemMessage(prompt);
            }

            history.AddUserMessage(request.UserMessage.Trim());

            var settings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var result = await _kernelPolicy
                .ExecuteAsync(async ct2 =>
                {
                    return await _chat.GetChatMessageContentAsync(
                        history,
                        executionSettings: settings,
                        kernel: _kernel,
                        cancellationToken: ct2);
                }, ct);

            if (result?.Content == null)
            {
                response.Status = "error during answer"; //todo add enum for reason error
                return response;

            }

            history.Add(result);
            var answer = new ChatMessage
            {
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Role = MessageRole.Assistant,
                Content = result.Content
            };

            response.Status = "ok";
            response.Answer = answer;
            return response;

        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error while processing request: {RequestId}", request.RequestId);
            response.Status = "error during answer";

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
}