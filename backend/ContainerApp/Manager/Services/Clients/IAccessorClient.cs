using Manager.Models;
using Manager.Models.Chat;

namespace Manager.Services.Clients;

public interface IAccessorClient
{
    Task<bool> UpdateTaskName(int id, string newTaskName);
    Task<bool> DeleteTask(int id);
    Task<(bool success, string message)> PostTaskAsync(TaskModel task);
    Task<TaskModel?> GetTaskAsync(int id);
    Task<IReadOnlyList<ChatMessage>> GetChatHistoryAsync(Guid threadId, CancellationToken ct = default);
    Task<ChatMessage?> StoreMessageAsync(ChatMessage msg, CancellationToken ct = default);
    Task<IReadOnlyList<ChatThread>> GetThreadsForUserAsync(string userId, CancellationToken ct = default);
}
