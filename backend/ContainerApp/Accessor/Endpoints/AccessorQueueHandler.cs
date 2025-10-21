using System.Text.Json;
using DotQueue;
using Accessor.Models;
using Accessor.Models.QueueMessages;
using Accessor.Services.Interfaces;
using Accessor.Exceptions;
 
namespace Accessor.Endpoints;

public class AccessorQueueHandler(
    ITaskService taskService,
    IManagerCallbackQueueService managerCallbackQueueService,
    ILogger<AccessorQueueHandler> logger) : RoutedQueueHandler<Message, MessageAction>(logger)
{
    protected override MessageAction GetAction(Message message) => message.ActionName;

    protected override void Configure(RouteBuilder routes) => routes
        .On(MessageAction.UpdateTask, HandleUpdateTaskAsync)
        .On(MessageAction.CreateTask, HandleCreateTaskAsync);

    private readonly ITaskService _taskService = taskService;
    private readonly IManagerCallbackQueueService _managerCallbackQueueService = managerCallbackQueueService;
    private readonly ILogger<AccessorQueueHandler> _logger = logger;

    private async Task HandleUpdateTaskAsync(Message message, IReadOnlyDictionary<string, string>? metadata, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var payload = message.Payload.Deserialize<TaskModel>();
            if (payload is null)
            {
                _logger.LogWarning("Invalid payload for UpdateTask");
                throw new DotQueue.NonRetryableException("Payload deserialization returned null for TaskModel.");
            }

            if (payload.Id <= 0)
            {
                _logger.LogWarning("Task Id must be a positive integer. Actual: {Id}", payload.Id);
                throw new DotQueue.NonRetryableException("Task Id must be a positive integer.");
            }

            if (string.IsNullOrWhiteSpace(payload.Name))
            {
                _logger.LogWarning("Task Name is required.");
                throw new DotQueue.NonRetryableException("Task Name is required.");
            }

            _logger.LogDebug("Processing task {Id}", payload.Id);
            var result = await _taskService.UpdateTaskNameAsync(
                payload.Id,
                payload.Name,
                ifMatch: null
            );
            _logger.LogInformation("Task {Id} processed", payload.Id);
        }
        catch (DotQueue.NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON payload for {Action}", message.ActionName);
            throw new DotQueue.NonRetryableException("Invalid JSON payload.", ex);
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Operation cancelled while processing {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Transient error while updating task for action {Action}", message.ActionName);
            throw new DotQueue.RetryableException("Transient error while updating task.", ex);
        }
    }

    private async Task HandleCreateTaskAsync(Message message, IReadOnlyDictionary<string, string>? metadata, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        TaskModel? taskModel = null;
        try
        {
            taskModel = message.Payload.Deserialize<TaskModel>();
            if (taskModel is null)
            {
                _logger.LogWarning("Invalid taskModel for CreateTask");
                throw new DotQueue.NonRetryableException("Payload deserialization returned null for TaskModel.");
            }

            UserContextMetadata? userContextMetadata = null;
            if (message.Metadata.HasValue)
            {
                userContextMetadata = JsonSerializer.Deserialize<UserContextMetadata>(message.Metadata.Value);
            }

            if (userContextMetadata is null)
            {
                _logger.LogWarning("Metadata is null for CreateTask action");
                throw new DotQueue.NonRetryableException("User Metadata is required for CreateTask action.");
            }

            if (taskModel.Id <= 0)
            {
                _logger.LogWarning("Task Id must be a positive integer. Actual: {Id}", taskModel.Id);
                throw new DotQueue.NonRetryableException("Task Id must be a positive integer.");
            }

            if (string.IsNullOrWhiteSpace(taskModel.Name))
            {
                _logger.LogWarning("Task Name is required.");
                throw new DotQueue.NonRetryableException("Task Name is required.");
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
        catch (DotQueue.NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Conflict creating task {Id}: {Message}", taskModel?.Id, ex.Message);
            throw new DotQueue.NonRetryableException(ex.Message, ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON taskModel for {Action}", message.ActionName);
            throw new DotQueue.NonRetryableException("Invalid JSON taskModel.", ex);
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Operation cancelled while processing {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Transient error while creating task for action {Action}", message.ActionName);
            throw new DotQueue.RetryableException("Transient error while creating task.", ex);
        }
    }
}
