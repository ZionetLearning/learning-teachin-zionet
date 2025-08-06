namespace Engine.Messaging;

public class QueueProcessor<T> : BackgroundService
{
    private readonly IQueueListener<T> _listener;
    private readonly IQueueHandler<T> _handler;
    public QueueProcessor(IQueueListener<T> listener, IQueueHandler<T> handler)
    {
        _listener = listener;
        _handler = handler;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        _listener.StartAsync((msg, renewLock, token) => _handler.HandleAsync(msg, renewLock, token), stoppingToken);
}
