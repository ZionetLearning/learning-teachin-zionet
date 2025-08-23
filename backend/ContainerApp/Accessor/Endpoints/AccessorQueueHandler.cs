using System.Text.Json;
using Accessor.Messaging;
using Accessor.Models;
using Accessor.Services;

namespace Accessor.Endpoints;

public class AccessorQueueHandler : IQueueHandler<Message>
{
    private readonly IAccessorService _accessorService;
    private readonly ILogger<AccessorQueueHandler> _logger;
    private readonly Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>> _handlers;

    public AccessorQueueHandler(IAccessorService accessorService, ILogger<AccessorQueueHandler> logger)
    {
        _accessorService = accessorService;
        _logger = logger;
        _handlers = new Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>>
        {
            [MessageAction.UpdateTask] = HandleUpdateTaskAsync,
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

            // Log metadata if exists
            if (message.Metadata != null && message.Metadata.Count > 0)
            {
                var metadataLog = string.Join(", ", message.Metadata.Select(kv => $"{kv.Key}={kv.Value}"));
                _logger.LogInformation("Received metadata for Task {Id}: {Metadata}", payload.Id, metadataLog);
            }
            else
            {
                _logger.LogWarning("No metadata received for Task {Id}", payload.Id);
            }

            _logger.LogDebug("Processing task {Id}", payload.Id);

            // Pass metadata forward so AccessorService can send callback
            await _accessorService.UpdateTaskNameAsync(payload.Id, payload.Name, message.Metadata);

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
}
