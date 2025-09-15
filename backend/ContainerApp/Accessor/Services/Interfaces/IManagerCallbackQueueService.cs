namespace Accessor.Services.Interfaces;

public interface IManagerCallbackQueueService
{
    Task PublishToManagerCallbackAsync<T>(T message, CancellationToken ct = default);
}