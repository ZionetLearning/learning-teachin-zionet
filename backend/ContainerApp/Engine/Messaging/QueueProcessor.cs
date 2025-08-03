namespace Engine.Messaging;

public class QueueProcessor<T> : BackgroundService
{
    private readonly IQueueListener<T> _listener;
    private readonly IQueueHandler<T> _handler;
    public QueueProcessor(IQueueListener<T> listener, IQueueHandler<T> handler)
    {
        this._listener = listener;
        this._handler = handler;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        this._listener.StartAsync((msg, token) => this._handler.HandleAsync(msg, token), stoppingToken);
}
