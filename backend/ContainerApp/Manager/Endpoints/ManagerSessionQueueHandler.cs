using DotQueue;
using Manager.Models.QueueMessages;

namespace Manager.Endpoints;

public class ManagerSessionQueueHandler : IQueueHandler<Message>
{
    private readonly ILogger<ManagerQueueHandler> _logger;
    private readonly Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>> _handlers;

    public ManagerSessionQueueHandler(ILogger<ManagerQueueHandler> logger)
    {
        _logger = logger;
        _handlers = new Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>>
        {
            //[MessageAction.SpeechToTextStream] = HandleSpeechToTextStreamAsync
        };
    }

    public async Task HandleAsync(Message message, IReadOnlyDictionary<string, string>? metadataCallback, Func<Task> renewLock, CancellationToken cancellationToken)
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
}
