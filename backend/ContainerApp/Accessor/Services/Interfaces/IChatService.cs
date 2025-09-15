using Accessor.Models;

namespace Accessor.Services.Interfaces;

public interface IChatService
{
    Task CreateChatAsync(ChatHistorySnapshot chat);
    Task<List<ChatSummaryDto>> GetChatsForUserAsync(Guid userId);
    Task<ChatHistorySnapshot?> GetHistorySnapshotAsync(Guid threadId);
    Task UpsertHistorySnapshotAsync(ChatHistorySnapshot snapshot);
}
