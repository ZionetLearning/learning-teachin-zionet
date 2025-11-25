using Engine.Models.Chat;
using Engine.Services.Clients.AccessorClient.Models;

namespace Engine.Services;

public interface IChatAiService
{
    Task<ChatAiServiceResponse> ChatHandlerAsync(EngineChatRequest request, HistorySnapshotDto historySnapshot, CancellationToken ct = default);

    IAsyncEnumerable<ChatAiStreamDelta> ChatStreamAsync(EngineChatRequest request, HistorySnapshotDto historySnapshot, CancellationToken ct = default);
}