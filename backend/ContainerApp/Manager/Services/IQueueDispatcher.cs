using Manager.Models.QueueMessages;

namespace Manager.Services;

public interface IQueueDispatcher
{
    Task SendAsync(string queueName, Message message, CancellationToken ct = default);
}