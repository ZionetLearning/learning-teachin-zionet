namespace Engine.Messaging
{
    public class QueueProcessor<T> : BackgroundService
    {
        readonly IQueueListener<T> _listener;
        readonly IQueueHandler<T> _handler;
        public QueueProcessor(IQueueListener<T> listener, IQueueHandler<T> handler)
        {
            _listener = listener;
            _handler = handler;
        }
        protected override Task ExecuteAsync(CancellationToken ct) =>
            _listener.StartAsync((msg, token) => _handler.HandleAsync(msg, token), ct);
    }
}
