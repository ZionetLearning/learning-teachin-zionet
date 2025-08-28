using Manager.Models.QueueMessages;

namespace Manager.Services;

public interface ICallbackDispatcher
{
    Task DispatchAsync(Message message, IReadOnlyDictionary<string, string>? metadataCallback, CancellationToken ct);
}