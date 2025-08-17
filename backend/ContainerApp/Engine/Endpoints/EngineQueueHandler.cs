using DotQueue;
using Engine.Constants;
using Engine.Helpers;
using Engine.Models;
using Engine.Models.Chat;
using Engine.Services;
using Engine.Services.Clients.AccessorClient;
using Engine.Services.Clients.AccessorClient.Models;

namespace Engine.Endpoints;

public class EngineQueueHandler : IQueueHandler<Message>
{
    private readonly IEngineService _engine;
    private readonly ILogger<EngineQueueHandler> _logger;
    private readonly Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>> _handlers;
    private readonly IChatAiService _aiService;
    private readonly IAiReplyPublisher _publisher;
    private readonly IAccessorClient _accessorClient;

    public EngineQueueHandler(
        IEngineService engine,
        IChatAiService aiService,
        IAiReplyPublisher publisher,
        IAccessorClient accessorClient,
        ILogger<EngineQueueHandler> logger)
    {
        _engine = engine;
        _aiService = aiService;
        _publisher = publisher;
        _accessorClient = accessorClient;
        _logger = logger;
        _handlers = new Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>>
        {
            [MessageAction.CreateTask] = HandleCreateTaskAsync,
            [MessageAction.TestLongTask] = HandleTestLongTaskAsync,
            [MessageAction.ProcessingQuestionAi] = HandleProcessingQuestionAiAsync

        };
    }

    public async Task HandleAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        if (_handlers.TryGetValue(message.ActionName, out var handler))
        {
            await handler(message, renewLock, cancellationToken);
        }
        else
        {
            _logger.LogWarning("No handler for action {Action}", message.ActionName);
            throw new NonRetryableException($"No handler for action {message.ActionName}");
        }
    }

    private async Task HandleCreateTaskAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var payload = PayloadValidation.DeserializeOrThrow<TaskModel>(message, _logger);

            PayloadValidation.ValidateTask(payload, _logger);

            _logger.LogDebug("Processing task {Id}", payload.Id);
            await _engine.ProcessTaskAsync(payload, cancellationToken);
            _logger.LogInformation("Task {Id} processed", payload.Id);
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Operation cancelled while processing message {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Transient error while processing task for action {Action}", message.ActionName);
            throw new RetryableException("Transient error while processing task.", ex);
        }
    }

    private async Task HandleTestLongTaskAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        using var renewalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var renewalTask = Task.Run(async () =>
        {
            try
            {
                while (!renewalCts.Token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(20), renewalCts.Token);
                    await renewLock();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Renew Lock loop error");
                throw;
            }
        }, renewalCts.Token);
        try
        {
            var payload = PayloadValidation.DeserializeOrThrow<TaskModel>(message, _logger);

            PayloadValidation.ValidateTask(payload, _logger);

            _logger.LogInformation("Starting long task handler for Task {Id}", payload.Id);

            await Task.Delay(TimeSpan.FromSeconds(80), cancellationToken);
            await _engine.ProcessTaskAsync(payload, cancellationToken);
            _logger.LogInformation("Task {Id} processed", payload.Id);

        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Operation cancelled while processing {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Transient error while processing long task for action {Action}", message.ActionName);
            throw new RetryableException("Transient error while processing long task.", ex);
        }
        finally
        {
            await renewalCts.CancelAsync();
            await renewalTask;
        }
    }
    private async Task HandleProcessingQuestionAiAsync(
        Message message,
        Func<Task> renewLock,
        CancellationToken ct)
    {
        try
        {
            var request = PayloadValidation.DeserializeOrThrow<EngineChatRequest>(message, _logger);

            using var _ = _logger.BeginScope(new { request.RequestId, request.ThreadId });

            if (string.IsNullOrWhiteSpace(request.UserMessage))
            {
                throw new NonRetryableException("UserMessage is required.");
            }

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                throw new NonRetryableException("UserId is required.");
            }

            if (request.TtlSeconds <= 0)
            {
                throw new NonRetryableException("TtlSeconds must be greater than 0.");
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now > request.SentAt + request.TtlSeconds)
            {
                _logger.LogWarning("Request {RequestId} is expired. Skipping.", request.RequestId);
                throw new NonRetryableException("Request TTL expired.");
            }

            var history = await _accessorClient.GetChatHistoryAsync(request.ThreadId, ct);

            var serviceRequest = new ChatAiServiseRequest
            {
                History = history,
                UserMessage = request.UserMessage,
                ChatType = request.ChatType,
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                RequestId = request.RequestId,
                SentAt = request.SentAt,
                TtlSeconds = request.TtlSeconds,
            };

            var aiResp = await _aiService.ChatHandlerAsync(serviceRequest, ct);

            if (aiResp.Status == "error" || aiResp.Answer == null)
            {
                var errorResponse = new EngineChatResponse
                {
                    RequestId = serviceRequest.RequestId,
                    Status = "error",
                    ThreadId = serviceRequest.ThreadId,
                    AssistantMessage = aiResp.Error
                };

                await _publisher.SendReplyAsync(errorResponse, $"{QueueNames.ManagerCallbackQueue}-out", ct);
                _logger.LogError("AI question {RequestId} failed", aiResp.RequestId);
                return;
            }

            var questionMessage = new ChatMessage
            {
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Role = MessageRole.User,
                Content = request.UserMessage
            };

            await _accessorClient.StoreMessageAsync(questionMessage, ct);

            await _accessorClient.StoreMessageAsync(aiResp.Answer, ct);

            var responseToManager = new EngineChatResponse
            {
                AssistantMessage = aiResp.Answer.Content,
                RequestId = aiResp.RequestId,
                Status = aiResp.Status,
                ThreadId = aiResp.ThreadId
            };

            await _publisher.SendReplyAsync(responseToManager, $"{QueueNames.ManagerCallbackQueue}-out", ct);
            _logger.LogInformation("AI question {RequestId} processed", aiResp.RequestId);
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (Exception ex)
        {
            if (ct.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Operation cancelled while processing {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, ct);
            }

            _logger.LogError(ex, "Transient error while processing AI question {Action}", message.ActionName);
            throw new RetryableException("Transient error while processing AI question.", ex);
        }
    }
}

