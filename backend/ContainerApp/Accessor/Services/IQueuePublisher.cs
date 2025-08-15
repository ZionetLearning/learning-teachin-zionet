namespace Accessor.Services;

public interface IQueuePublisher
{
    Task PublishAsync<T>(string queueName, T message, CancellationToken ct = default);
}