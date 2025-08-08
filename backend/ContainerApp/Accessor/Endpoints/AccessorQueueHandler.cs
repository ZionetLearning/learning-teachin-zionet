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

    // add erros to retry or not retry that the queue listener will catch
    public async Task HandleAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        if (_handlers.TryGetValue(message.ActionName, out var handler))
        {
            await handler(message, renewLock, cancellationToken);
        }
        else
        {
            _logger.LogWarning("No handler for action {Action}", message.ActionName);
        }
    }

    private async Task HandleUpdateTaskAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        var payload = message.Payload.Deserialize<TaskModel>();
        if (payload is null)
        {
            _logger.LogWarning("Invalid payload for CreateTask");
            return;
        }

        _logger.LogDebug("Processing task {Id}", payload.Id);
        await _accessorService.UpdateTaskNameAsync(payload.Id, payload.Name);
        _logger.LogInformation("Task {Id} processed", payload.Id);
    }
}
