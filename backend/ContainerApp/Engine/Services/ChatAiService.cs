using System.Runtime.CompilerServices;
using System.Text;
using Engine.Helpers;
using Engine.Models.Chat;
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
        _log.LogInformation("ChatAI request started {RequestId} for User {UserId}, Thread {ThreadId}, Type {ChatType}, TTL {TtlSeconds}",
    request.RequestId, request.UserId, request.ThreadId, request.ChatType, request.TtlSeconds);
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

            _log.LogInformation("LLM request {RequestId} finished model {Model}", request.RequestId, settings.ModelId);

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

    public async IAsyncEnumerable<ChatAiStreamDelta> ChatStreamAsync(
    ChatAiServiseRequest request,
    [EnumeratorCancellation] CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now > request.SentAt + request.TtlSeconds)
        {
            yield return new ChatAiStreamDelta { RequestId = request.RequestId, ThreadId = request.ThreadId, UserId = request.UserId, IsFinal = true, Stage = ChatStreamStage.Expired };
            yield break;
        }

        var settings = new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var sb = new StringBuilder();
        StreamingChatMessageContent? lastPart = null;

        _log.LogInformation("Starting streaming for {RequestId}", request.RequestId);

        await foreach (var part in
            _chat.GetStreamingChatMessageContentsAsync(request.History, settings, _kernel, ct))
        {
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            lastPart = part;

            foreach (var t in part.Items.OfType<StreamingTextContent>())
            {
                if (!string.IsNullOrEmpty(t.Text))
                {
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
                }
            }

            foreach (var call in part.Items.OfType<StreamingFunctionCallUpdateContent>())
            {
                yield return new ChatAiStreamDelta
                {
                    RequestId = request.RequestId,
                    ThreadId = request.ThreadId,
                    UserId = request.UserId,
                    ToolCall = call.Name,
                    Stage = ChatStreamStage.Tool,
                    IsFinal = false
                };
            }
        }

        var accumulated = sb.ToString();

        var finalMessage = new ChatMessageContent(
            AuthorRole.Assistant,
            accumulated,
            modelId: lastPart?.ModelId ?? string.Empty
        );

        if (lastPart?.Metadata is not null)
        {
            finalMessage.Metadata = new Dictionary<string, object?>(lastPart.Metadata);
        }

        request.History.Add(finalMessage);

        var updatedHistory = HistoryMapper.CloneToChatHistory(request.History);

        yield return new ChatAiStreamDelta
        {
            RequestId = request.RequestId,
            ThreadId = request.ThreadId,
            UserId = request.UserId,
            Delta = null,
            IsFinal = true,
            Stage = ChatStreamStage.Final,
            UpdatedHistory = updatedHistory
        };
    }
}