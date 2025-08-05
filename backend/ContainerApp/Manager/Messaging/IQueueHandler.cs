namespace Manager.Messaging;

public interface IQueueHandler<T>
{
    Task HandleAsync(T message, CancellationToken cancellationToken);
}
