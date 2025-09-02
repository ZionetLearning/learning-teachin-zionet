using System.Text.Json;
using DotQueue;
using Accessor.Models;
using Accessor.Models.QueueMessages;
using Accessor.Services;
using Accessor.Constants;
using Accessor.Routing;
using Dapr.Client;

namespace Accessor.Endpoints;

public class AccessorQueueHandler : IQueueHandler<Message>
{
    private readonly IAccessorService _accessorService;
    private readonly IManagerCallbackQueueService _managerCallbackQueueService;
    private readonly ILogger<AccessorQueueHandler> _logger;
    private readonly DaprClient _daprClient;
    private readonly IQueueDispatcher _queueDispatcher;
    private readonly RoutingMiddleware _routingMiddleware;
    private readonly Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>> _handlers;

    public AccessorQueueHandler(
        IAccessorService accessorService,
        IManagerCallbackQueueService managerCallbackQueueService,
        ILogger<AccessorQueueHandler> logger,
        DaprClient daprClient,
        IQueueDispatcher queueDispatcher,
        RoutingMiddleware routingMiddleware)
    {
        _accessorService = accessorService;
        _managerCallbackQueueService = managerCallbackQueueService;
        _daprClient = daprClient;
        _logger = logger;
        _queueDispatcher = queueDispatcher;
        _routingMiddleware = routingMiddleware;

        _handlers = new Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>>
        {
            [MessageAction.UpdateTask] = HandleUpdateTaskAsync,
            [MessageAction.CreateTask] = HandleCreateTaskAsync
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

    private async Task HandleUpdateTaskAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var payload = message.Payload.Deserialize<TaskModel>();
            if (payload is null)
            {
                _logger.LogWarning("Invalid payload for UpdateTask");
                throw new NonRetryableException("Payload deserialization returned null for TaskModel.");
            }

            if (payload.Id <= 0)
            {
                _logger.LogWarning("Task Id must be a positive integer. Actual: {Id}", payload.Id);
                throw new NonRetryableException("Task Id must be a positive integer.");
            }

            if (string.IsNullOrWhiteSpace(payload.Name))
            {
                _logger.LogWarning("Task Name is required.");
                throw new NonRetryableException("Task Name is required.");
            }

            _logger.LogDebug("Processing task {Id}", payload.Id);

            await _accessorService.UpdateTaskNameAsync(payload.Id, payload.Name);

            // build TaskResult
            var result = new TaskResult(payload.Id, TaskResultStatus.Updated);
            var resultMessage = new Message
            {
                ActionName = MessageAction.TaskResult,
                Payload = JsonSerializer.SerializeToElement(result)
            };

            var ctx = _routingMiddleware.Accessor.Current;
            _logger.LogInformation(
                "[ACCESSOR:OUTBOUND] Forwarding TaskResult with Callback={Callback}, ReplyQueue={ReplyQueue}",
                ctx?.CallbackMethod ?? "(none)",
                ctx?.ReplyQueue ?? "(none)");

            //Dispatcher automatically attaches routing headers
            await _queueDispatcher.SendAsync(QueueNames.ManagerCallbackQueue, resultMessage, cancellationToken);

            _logger.LogInformation("Task {Id} processed", payload.Id);
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
                _logger.LogWarning(ex, "Operation cancelled while processing {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Transient error while updating task for action {Action}", message.ActionName);
            throw new RetryableException("Transient error while updating task.", ex);
        }
    }

    private async Task HandleCreateTaskAsync(Message message, Func<Task> func, CancellationToken cancellationToken)
    {
        try
        {
            var taskModel = message.Payload.Deserialize<TaskModel>();
            if (taskModel is null)
            {
                _logger.LogWarning("Invalid taskModel for CreateTask");
                throw new NonRetryableException("Payload deserialization returned null for TaskModel.");
            }

            UserContextMetadata? metadata = null;
            if (message.Metadata.HasValue)
            {
                metadata = JsonSerializer.Deserialize<UserContextMetadata>(message.Metadata.Value);
            }

            if (metadata is null)
            {
                _logger.LogWarning("Metadata is null for CreateTask action");
                throw new NonRetryableException("User Metadata is required for CreateTask action.");
            }

            if (taskModel.Id <= 0)
            {
                _logger.LogWarning("Task Id must be a positive integer. Actual: {Id}", taskModel.Id);
                throw new NonRetryableException("Task Id must be a positive integer.");
            }

            if (string.IsNullOrWhiteSpace(taskModel.Name))
            {
                _logger.LogWarning("Task Name is required.");
                throw new NonRetryableException("Task Name is required.");
            }

            _logger.LogDebug("Creating task {Id}", taskModel.Id);

            await _accessorService.CreateTaskAsync(taskModel);

            // build TaskResult
            var result = new TaskResult(taskModel.Id, TaskResultStatus.Created);

            var resultMessage = new Message
            {
                ActionName = MessageAction.TaskResult,
                Payload = JsonSerializer.SerializeToElement(result)
            };

            var ctx = _routingMiddleware.Accessor.Current;
            _logger.LogInformation(
                "[ACCESSOR:OUTBOUND] Forwarding TaskResult with Callback={Callback}, ReplyQueue={ReplyQueue}",
                ctx?.CallbackMethod ?? "(none)",
                ctx?.ReplyQueue ?? "(none)");

            await _queueDispatcher.SendAsync(QueueNames.ManagerCallbackQueue, resultMessage, cancellationToken);

            var notification = new Notification
            {
                Message = $"Task {taskModel.Name} created successfully.",
                Type = NotificationType.Success
            };

            var messageToManger = new Message
            {
                ActionName = MessageAction.NotifyUser,
                Payload = JsonSerializer.SerializeToElement(notification),
                Metadata = message.Metadata
            };

            await _managerCallbackQueueService.PublishToManagerCallbackAsync(messageToManger, cancellationToken);

            _logger.LogInformation("Task {Id} created", taskModel.Id);
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON taskModel for {Action}", message.ActionName);
            throw new NonRetryableException("Invalid JSON taskModel.", ex);
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Operation cancelled while processing {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Transient error while creating task for action {Action}", message.ActionName);
            throw new RetryableException("Transient error while creating task.", ex);
        }
    }
}
