using Engine.Services.Clients.AccessorClient.Models;

namespace Engine.Services.Clients.AccessorClient;

public interface IAccessorClient
{
    Task<IReadOnlyList<ChatMessage>> GetChatHistoryAsync(Guid threadId, CancellationToken ct = default);
    Task<HistorySnapshotDto> GetHistorySnapshotAsync(Guid threadId, Guid userId, CancellationToken ct = default);
    Task<HistorySnapshotDto> UpsertHistorySnapshotAsync(UpsertHistoryRequest request, CancellationToken ct = default);
}

