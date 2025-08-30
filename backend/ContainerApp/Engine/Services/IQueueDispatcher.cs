using Engine.Models.QueueMessages;

namespace Engine.Services;

public interface IQueueDispatcher
{
    Task SendAsync(string queueName, Message message, CancellationToken ct = default);
}