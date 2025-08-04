namespace Accessor.Messaging
{
    public interface IQueueListener<T>
    {
        Task StartAsync(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken);
    }
}
