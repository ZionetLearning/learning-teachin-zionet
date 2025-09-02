using Engine.Models.Chat;
using Engine.Helpers;
using Engine.Services.Clients.AccessorClient.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
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
            var storyForKernel = HistoryMapper.CloneToChatHistory(request.History);

            var settings = new AzureOpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var result = await _kernelPolicy.ExecuteAsync(
                async ct2 => await _chat.GetChatMessageContentAsync(request.History, settings, _kernel, ct2),
                ct);

            if (string.IsNullOrWhiteSpace(result?.Content))
            {
                response.Status = ChatAnswerStatus.Fail;
                response.Error = "Empty model result";
                return response;
            }

            request.History.Add(result);

            response.Status = ChatAnswerStatus.Ok;
            response.Answer = new ChatMessage
            {
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Role = MessageRole.Assistant,
                Content = result.Content!
            };
            response.UpdatedHistory = storyForKernel;
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
}

