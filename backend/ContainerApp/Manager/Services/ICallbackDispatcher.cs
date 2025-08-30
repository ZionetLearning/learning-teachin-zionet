using Manager.Models.QueueMessages;

namespace Manager.Services;

public interface ICallbackDispatcher
{
    Task DispatchAsync(Message message, CancellationToken ct);
}