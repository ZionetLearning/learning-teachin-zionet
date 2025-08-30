using System.Text.Json;
using DotQueue;
using Manager.Models;
using Manager.Models.Chat;
using Manager.Models.ModelValidation;
using Manager.Models.Notifications;
using Manager.Models.QueueMessages;
using Manager.Services;
using Manager.Routing;

namespace Manager.Endpoints;

public class ManagerQueueHandler : IQueueHandler<Message>
{
    private readonly IAiGatewayService _aiService;
    private readonly IManagerService _managerService;
    private readonly ICallbackDispatcher _callbackDispatcher;
    private readonly ILogger<ManagerQueueHandler> _logger;
    private readonly RoutingMiddleware _routingMiddleware;
    private readonly Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>> _handlers;

    public ManagerQueueHandler(
        IAiGatewayService aiService,
        ILogger<ManagerQueueHandler> logger,
        IManagerService managerService,
        ICallbackDispatcher callbackDispatcher,
        RoutingMiddleware routingMiddleware)
    {
        _aiService = aiService;
        _logger = logger;
        _managerService = managerService;
        _callbackDispatcher = callbackDispatcher;
        _routingMiddleware = routingMiddleware;

        _handlers = new Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>>
        {
            [MessageAction.AnswerAi] = HandleAnswerAiAsync,
            [MessageAction.NotifyUser] = HandleNotifyUserAsync,
            [MessageAction.ProcessingChatMessage] = HandleAIChatAnswerAsync,
            [MessageAction.TaskResult] = HandleTaskResultAsync
        };
    }

    public async Task HandleAsync(Message message, IReadOnlyDictionary<string, string>? metadataCallback, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        await _routingMiddleware.HandleAsync(
            message,
            metadataCallback,
            async () =>
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
            });
    }

    private async Task HandleTaskResultAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        var ctx = _routingMiddleware.Accessor.Current;

        if (ctx != null)
        {
            _logger.LogInformation(
                "[RESULT] TaskResult received → Callback={Callback}, ReplyQueue={ReplyQueue}",
                ctx.CallbackMethod ?? "(none)",
                ctx.ReplyQueue ?? "(none)");
        }
        else
        {
            _logger.LogWarning("[RESULT] RoutingContext is null!");
        }

        _logger.LogInformation("Dispatching TaskResult...");
        await _callbackDispatcher.DispatchAsync(message, cancellationToken);
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
            var notification = message.Payload.Deserialize<UserNotification>();
            if (notification is null)
            {
                _logger.LogError("Payload deserialization returned null for UserNotification.");
                throw new NonRetryableException("Payload deserialization returned null for UserNotification.");
            }

            UserContextMetadata? metadata = null;
            if (message.Metadata.HasValue)
            {
                metadata = JsonSerializer.Deserialize<UserContextMetadata>(message.Metadata.Value);
            }

            if (metadata is null)
            {
                _logger.LogWarning("Metadata is null for NotifyUser action");
                throw new NonRetryableException("User Metadata is required for NotifyUser action.");
            }

            if (!ValidationExtensions.TryValidate(notification, out var validationErrors))
            {
                _logger.LogWarning("Validation failed for {Model}: {Errors}",
                    nameof(UserNotification), validationErrors);
                throw new NonRetryableException(
                    $"Validation failed for {nameof(UserNotification)}: {string.Join("; ", validationErrors)}");
            }

            _logger.LogInformation("Processing notification {MessageId} for user {UserId}", metadata.MessageId, metadata.UserId);
            await _managerService.SendUserNotificationAsync(metadata.UserId, notification);
            _logger.LogInformation("Notification processed for user {UserId}", metadata.UserId);
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

    public async Task HandleAIChatAnswerAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var chatResponse = message.Payload.Deserialize<AIChatResponse>();
            if (chatResponse is null)
            {
                _logger.LogError("Payload deserialization returned null for EngineChatResponse.");
                throw new NonRetryableException("Payload deserialization returned null for EngineChatResponse.");
            }

            UserContextMetadata? metadata = null;
            if (message.Metadata.HasValue)
            {
                metadata = JsonSerializer.Deserialize<UserContextMetadata>(message.Metadata.Value);
            }

            if (metadata is null)
            {
                _logger.LogWarning("Metadata is null for ProcessingChatMessage action");
                throw new NonRetryableException("Chat Metadata is required for ProcessingChatMessage action.");
            }

            if (!ValidationExtensions.TryValidate(chatResponse, out var validationErrors))
            {
                _logger.LogWarning("Validation failed for {Model}: {Errors}",
                    nameof(AIChatResponse), validationErrors);
                throw new NonRetryableException(
                    $"Validation failed for {nameof(AIChatResponse)}: {string.Join("; ", validationErrors)}");
            }

            var userEvent = new UserEvent<AIChatResponse>
            {
                EventType = EventType.ChatAiAnswer,
                Payload = chatResponse,
            };

            await _managerService.SendUserEventAsync(metadata.UserId, userEvent);

        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON chat response for {Action}", message.ActionName);
            throw new NonRetryableException("Invalid JSON chat response.", ex);
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Operation cancelled while processing message {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Error processing AI chat answer");
            throw new RetryableException("Transient error while processing AI chat answer.", ex);
        }
    }
}
