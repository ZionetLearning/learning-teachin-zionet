using Accessor.Models.QueueMessages;

namespace Accessor.Services;

public interface IQueueDispatcher
{
    Task SendAsync(string queueName, Message message, CancellationToken ct = default);
}