namespace Accessor.Messaging;

public class QueueProcessor<T> : BackgroundService
{
    private readonly IQueueListener<T> _listener;
    private readonly IServiceScopeFactory _scopeFactory;

    public QueueProcessor(IQueueListener<T> listener, IServiceScopeFactory scopeFactory)
    {
        _listener = listener;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        _listener.StartAsync(ProcessMessageAsync, stoppingToken);

    private async Task ProcessMessageAsync(T msg, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueueHandler<T>>();
        await handler.HandleAsync(msg, ct);
    }
}
