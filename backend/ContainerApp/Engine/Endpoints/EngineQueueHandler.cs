using DotQueue;
using Engine.Helpers;
using Engine.Models;
using Engine.Models.Chat;
using Engine.Models.QueueMessages;
using Engine.Models.Sentences;
using Engine.Services;
using Engine.Services.Clients.AccessorClient;
using Engine.Services.Clients.AccessorClient.Models;
using Dapr.Client;

namespace Engine.Endpoints;

public class EngineQueueHandler : IQueueHandler<Message>
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<EngineQueueHandler> _logger;
    private readonly Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>> _handlers;
    private readonly IChatAiService _aiService;
    private readonly ISentencesService _sentencesService;
    private readonly IAiReplyPublisher _publisher;
    private readonly IAccessorClient _accessorClient;

    public EngineQueueHandler(
        DaprClient daprClient,
        IChatAiService aiService,
        IAiReplyPublisher publisher,
        IAccessorClient accessorClient,
        ILogger<EngineQueueHandler> logger,
        ISentencesService sentencesService)
    {
        _daprClient = daprClient;
        _aiService = aiService;
        _publisher = publisher;
        _accessorClient = accessorClient;
        _logger = logger;
        _sentencesService = sentencesService;
        _handlers = new Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>>
        {
            [MessageAction.CreateTask] = HandleCreateTaskAsync,
            [MessageAction.TestLongTask] = HandleTestLongTaskAsync,
            [MessageAction.ProcessingChatMessage] = HandleProcessingChatMessageAsync,
            [MessageAction.GenerateSentences] = HandleSentenceGenerationAsync

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

            using var _ = _logger.BeginScope("Processing TaskId: {TaskId}", payload.Id);
            _logger.LogInformation("Inside {Method}", nameof(HandleCreateTaskAsync));

            cancellationToken.ThrowIfCancellationRequested();

            if (payload is null)
            {
                _logger.LogWarning("Attempted to process a null task");
                throw new ArgumentNullException(nameof(message), "Task payload cannot be null");
            }

            _logger.LogInformation("Logged task: {Name}", payload.Name);

            await _daprClient.InvokeMethodAsync(HttpMethod.Post, "accessor", "tasks-accessor/task", payload, cancellationToken);

            _logger.LogInformation("Task {Id} forwarded to the Accessor service", payload.Id);
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

            await Task.Delay(TimeSpan.FromSeconds(80), cancellationToken);

            using var _ = _logger.BeginScope("Starting long task handler for Task {Id}", payload.Id);
            _logger.LogInformation("Inside {Method}", nameof(HandleTestLongTaskAsync));

            cancellationToken.ThrowIfCancellationRequested();

            if (payload is null)
            {
                _logger.LogWarning("Attempted to process a null task");
                throw new ArgumentNullException(nameof(message), "Task payload cannot be null");
            }

            _logger.LogInformation("Logged task: {Name}", payload.Name);

            await _daprClient.InvokeMethodAsync(
                HttpMethod.Post, "accessor", "tasks-accessor/task", payload, cancellationToken);

            _logger.LogInformation("Task {Id} forwarded to the Accessor service", payload.Id);
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

    private async Task HandleProcessingChatMessageAsync(
        Message message,
        Func<Task> renewLock,
        CancellationToken ct)
    {
        try
        {
            var request = PayloadValidation.DeserializeOrThrow<EngineChatRequest>(message, _logger);
            PayloadValidation.ValidateEngineChatRequest(request, _logger);

            var userContext = MetadataValidation.DeserializeOrThrow<UserContextMetadata>(message, _logger);
            MetadataValidation.ValidateUserContext(userContext, _logger);

            using var _ = _logger.BeginScope(new { request.RequestId, request.ThreadId });

            if (request.UserId == Guid.Empty)
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
                _logger.LogWarning("Chat request {RequestId} expired. Skipping.", request.RequestId);
                throw new NonRetryableException("Request TTL expired.");
            }

            var snapshot = await _accessorClient.GetHistorySnapshotAsync(request.ThreadId, request.UserId, ct);

            var serviceRequest = new ChatAiServiseRequest
            {
                History = snapshot.History,
                UserMessage = request.UserMessage,
                ChatType = request.ChatType,
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Name = snapshot.Name,
                RequestId = request.RequestId,
                SentAt = request.SentAt,
                TtlSeconds = request.TtlSeconds,
            };

            var aiResponse = await _aiService.ChatHandlerAsync(serviceRequest, ct);

            if (aiResponse.Status != ChatAnswerStatus.Ok || aiResponse.Answer is null)
            {
                var errorResponse = new EngineChatResponse
                {
                    RequestId = serviceRequest.RequestId,
                    Status = ChatAnswerStatus.Fail,
                    ChatName = aiResponse.Name,
                    ThreadId = serviceRequest.ThreadId,
                    AssistantMessage = aiResponse.Error
                };

                await _publisher.SendReplyAsync(userContext, errorResponse, ct);
                _logger.LogError("Chat request {RequestId} failed: {Error}", aiResponse.RequestId, aiResponse.Error);
                return;
            }

            var upsert = new UpsertHistoryRequest
            {
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Name = aiResponse.Name,
                ChatType = request.ChatType.ToString().ToLowerInvariant(),
                History = aiResponse.UpdatedHistory
            };

            await _accessorClient.UpsertHistorySnapshotAsync(upsert, ct);

            var responseToManager = new EngineChatResponse
            {
                AssistantMessage = aiResponse.Answer.Content,
                RequestId = aiResponse.RequestId,
                ChatName = aiResponse.Name,
                Status = aiResponse.Status,
                ThreadId = aiResponse.ThreadId
            };

            await _publisher.SendReplyAsync(userContext, responseToManager, ct);
            _logger.LogInformation("Chat request {RequestId} processed successfully", aiResponse.RequestId);
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

            _logger.LogError(ex, "Transient error while processing AI chat {Action}", message.ActionName);
            throw new RetryableException("Transient error while processing AI chat.", ex);
        }
    }
    private async Task HandleSentenceGenerationAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var payload = PayloadValidation.DeserializeOrThrow<SentenceRequest>(message, _logger);

            PayloadValidation.ValidateSentenceGenerationRequest(payload, _logger);

            _logger.LogDebug("Processing sentence generation");
            var response = await _sentencesService.GenerateAsync(payload, cancellationToken);
            var userId = payload.UserId;
            await _publisher.SendGeneratedMessagesAsync(userId.ToString(), response, cancellationToken);
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

            _logger.LogError(ex, "Transient error while processing for action {Action}", message.ActionName);
            throw new RetryableException("Transient error while processing.", ex);
        }
    }
}
