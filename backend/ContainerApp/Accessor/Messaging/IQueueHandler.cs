namespace Accessor.Messaging;

public interface IQueueHandler<T>
{
    Task HandleAsync(T message, Func<Task> renewLock, CancellationToken cancellationToken);
}
