using System.Text.Json;
using DotQueue;
using Accessor.Models;
using Accessor.Models.QueueMessages;
using Accessor.Services.Interfaces;

namespace Accessor.Endpoints;

public class AccessorQueueHandler : IQueueHandler<Message>
{
    private readonly ITaskService _taskService;
    private readonly IManagerCallbackQueueService _managerCallbackQueueService;
    private readonly ILogger<AccessorQueueHandler> _logger;
    private readonly Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>> _handlers;

    public AccessorQueueHandler(
        ITaskService taskService,
        IManagerCallbackQueueService managerCallbackQueueService,
        ILogger<AccessorQueueHandler> logger)
    {
        _taskService = taskService;
        _managerCallbackQueueService = managerCallbackQueueService;
        _logger = logger;
        _handlers = new Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>>
        {
            [MessageAction.UpdateTask] = HandleUpdateTaskAsync,
            [MessageAction.CreateTask] = HandleCreateTaskAsync
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
            var result = await _taskService.UpdateTaskNameAsync(
                payload.Id,
                payload.Name,
                ifMatch: null
            );
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

            await _taskService.CreateTaskAsync(taskModel);

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
