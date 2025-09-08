using DotQueue;
using Manager.Models.QueueMessages;

namespace Manager.Endpoints;

public class ManagerSessionQueueHandler : IQueueHandler<SessionQueueMessage>
{
    private readonly ILogger<ManagerSessionQueueHandler> _logger;
    private readonly Dictionary<MessageSessionAction, Func<SessionQueueMessage, Func<Task>, IReadOnlyDictionary<string, string>?, CancellationToken, Task>> _handlers;

    public ManagerSessionQueueHandler(ILogger<ManagerSessionQueueHandler> logger)
    {
        _logger = logger;
        _handlers = new Dictionary<MessageSessionAction, Func<SessionQueueMessage, Func<Task>, IReadOnlyDictionary<string, string>?, CancellationToken, Task>>
        {
            // Dummy handlers for future actions
            [MessageSessionAction.ChatStream] = (msg, renew, metadata, ct) => Task.CompletedTask,
        };
    }

    public async Task HandleAsync(SessionQueueMessage message, IReadOnlyDictionary<string, string>? metadataCallback, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        if (_handlers.TryGetValue(message.ActionName, out var handler))
        {
            await handler(message, renewLock, metadataCallback, cancellationToken);
            return;
        }

        _logger.LogWarning("No handler for action {Action}", message.ActionName);

        throw new NonRetryableException($"No handler for action {message.ActionName}");
    }
}
