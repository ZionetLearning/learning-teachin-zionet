using Manager.Models;
using Manager.Models.Chat;
using Manager.Models.Users;

namespace Manager.Services.Clients;

public interface IAccessorClient
{
    Task<bool> UpdateTaskName(int id, string newTaskName, IReadOnlyDictionary<string, string>? metadataCallback = null);
    Task<bool> DeleteTask(int id);
    Task<(bool success, string message)> PostTaskAsync(TaskModel task, IReadOnlyDictionary<string, string>? metadataCallback = null);
    Task<TaskModel?> GetTaskAsync(int id);
    Task<IReadOnlyList<ChatMessage>> GetChatHistoryAsync(Guid threadId, CancellationToken ct = default);
    Task<ChatMessage?> StoreMessageAsync(ChatMessage msg, CancellationToken ct = default);
    Task<IReadOnlyList<ChatThread>> GetThreadsForUserAsync(string userId, CancellationToken ct = default);
    Task<UserModel?> GetUserAsync(Guid userId);
    Task<bool> CreateUserAsync(UserModel user);
    Task<bool> UpdateUserAsync(UpdateUserModel user, Guid userId);
    Task<bool> DeleteUserAsync(Guid userId);
}
