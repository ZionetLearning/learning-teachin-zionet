using Engine.Services.Clients.AccessorClient.Models;

namespace Engine.Services.Clients.AccessorClient;

public interface IAccessorClient
{
    Task<IReadOnlyList<ChatMessage>> GetChatHistoryAsync(Guid threadId, CancellationToken ct = default);
    Task<ChatMessage?> StoreMessageAsync(ChatMessage msg, CancellationToken ct = default);
    Task<IReadOnlyList<ChatThread>> GetThreadsForUserAsync(string userId, CancellationToken ct = default);
    Task<HistorySnapshotDto> GetHistorySnapshotAsync(Guid threadId, CancellationToken ct = default);
    Task<HistorySnapshotDto> UpsertHistorySnapshotAsync(UpsertHistoryRequest request, CancellationToken ct = default);
}

