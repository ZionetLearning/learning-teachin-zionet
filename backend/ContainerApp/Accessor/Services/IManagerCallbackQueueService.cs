namespace Accessor.Services;

public interface IManagerCallbackQueueService
{
    Task PublishToManagerCallbackAsync<T>(T message, CancellationToken ct = default);
}