using System.Text.Json;
using Manager.Messaging;
using Manager.Models;
using Manager.Models.ModelValidation;
using Manager.Models.QueueMessages;
using Manager.Services;

namespace Manager.Endpoints;
public class ManagerQueueHandler : IQueueHandler<Message>
{
    private readonly IAiGatewayService _aiService;
    private readonly IManagerService _managerService;
    private readonly ILogger<ManagerQueueHandler> _logger;
    private readonly Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>> _handlers;

    public ManagerQueueHandler(
        IAiGatewayService aiService,
        ILogger<ManagerQueueHandler> logger,
        IManagerService managerService)
    {
        _managerService = managerService;
        _aiService = aiService;
        _logger = logger;
        _handlers = new Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>>
        {
            [MessageAction.AnswerAi] = HandleAnswerAiAsync,
            [MessageAction.NotifyUser] = HandleNotifyUserAsync,
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

    public async Task HandleNotifyUserAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            if (message is not UserContextMessage userContextMessage)
            {
                throw new NonRetryableException("NotifyUser requires user context");
            }

            var notification = message.Payload.Deserialize<UserNotification>();
            if (notification is null)
            {
                _logger.LogError("Payload deserialization returned null for UserNotification.");
                throw new NonRetryableException("Payload deserialization returned null for UserNotification.");
            }

            if (!ValidationExtensions.TryValidate(notification, out var validationErrors))
            {
                _logger.LogWarning("Validation failed for {Model}: {Errors}",
                    nameof(UserNotification), validationErrors);
                throw new NonRetryableException(
                    $"Validation failed for {nameof(UserNotification)}: {string.Join("; ", validationErrors)}");
            }

            _logger.LogInformation("Processing notification for user {UserId}", userContextMessage.UserId);
            await _managerService.SendUserNotificationAsync(userContextMessage.UserId, notification);
            _logger.LogInformation("Notification processed for user {UserId}", userContextMessage.UserId);
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON notification for {Action}", message.ActionName);
            throw new NonRetryableException("Invalid JSON notification.", ex);
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Operation cancelled while processing message {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Error notifying user");
            throw new RetryableException("Transient error while notifying user.", ex);
        }
    }
}
