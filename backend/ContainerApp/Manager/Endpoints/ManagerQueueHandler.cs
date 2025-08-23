using Manager.Models;
using Manager.Models.ModelValidation;
using Manager.Services;
using Manager.Messaging;
using Manager.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace Manager.Endpoints;
public class ManagerQueueHandler : IQueueHandler<Message>
{
    private readonly IAiGatewayService _aiService;
    private readonly IManagerService _managerService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<ManagerQueueHandler> _logger;
    private readonly Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>> _handlers;

    public ManagerQueueHandler(
        IAiGatewayService aiService,
        IManagerService managerService,
        IHubContext<NotificationHub> hubContext,
        ILogger<ManagerQueueHandler> logger)
    {
        _aiService = aiService;
        _managerService = managerService;
        _hubContext = hubContext;
        _logger = logger;
        _handlers = new Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>>
        {
            [MessageAction.AnswerAi] = HandleAnswerAiAsync,
            [MessageAction.TaskResult] = HandleTaskResultAsync

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

    public async Task HandleTaskResultAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var result = message.Payload.Deserialize<TaskResult>();
            if (result == null)
            {
                _logger.LogError("Payload deserialization returned null for TaskResult.");
                throw new NonRetryableException("Payload deserialization returned null for TaskResult.");
            }

            _logger.LogInformation("[CALLBACK via QueueHandler] Task {Id} finished with status: {Status}",
                result.Id, result.Status);

            _logger.LogInformation("Sending TaskUpdated for TaskId {Id}, Status {Status} to all clients", result.Id, result.Status);

            await _hubContext.Clients.All.SendAsync(
                "TaskUpdated",
                new TaskUpdateMessage { TaskId = result.Id, Status = result.Status },
                cancellationToken
            );

            _logger.LogInformation("TaskUpdated event dispatched via SignalR for TaskId {Id}", result.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling TaskResult callback");
            throw;
        }
    }

    public async Task HandleAnswerAiAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var response = message.Payload.Deserialize<AiResponseModel>();
            if (response is null)
            {
                _logger.LogError("Payload deserialization returned null for AiResponseModel.");
                throw new NonRetryableException("Payload deserialization returned null for AiResponseModel.");
            }

            _logger.LogInformation("Received AI answer {Id} from engine", response!.Id);

            if (!ValidationExtensions.TryValidate(response, out var validationErrors))
            {
                _logger.LogWarning("Validation failed for {Model}: {Errors}",
                    nameof(AiResponseModel), validationErrors);
                throw new NonRetryableException(
                    $"Validation failed for {nameof(AiResponseModel)}: {string.Join("; ", validationErrors)}");
            }

            await _aiService.SaveAnswerAsync(response, cancellationToken);
            _logger.LogInformation("Answer {Id} saved", response.Id);
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON payload for {Action}", message.ActionName);
            throw new NonRetryableException("Invalid JSON payload.", ex);
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Operation cancelled while processing message {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Error saving answer");
            throw new RetryableException("Transient error while saving answer.", ex);
        }
    }
}
