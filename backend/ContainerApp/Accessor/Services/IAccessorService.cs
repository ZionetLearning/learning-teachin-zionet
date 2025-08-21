using Accessor.Models;
using Accessor.Models.Users;

namespace Accessor.Services;

public interface IAccessorService
{
    Task InitializeAsync();
    Task<TaskModel?> GetTaskByIdAsync(int id);
    Task CreateTaskAsync(TaskModel task);
    Task<bool> DeleteTaskAsync(int taskId);
    Task<bool> UpdateTaskNameAsync(int taskId, string newName);
    Task<ChatThread?> GetThreadByIdAsync(Guid threadId);
    Task CreateThreadAsync(ChatThread thread);
    Task AddMessageAsync(ChatMessage message);
    Task<IEnumerable<ChatMessage>> GetMessagesByThreadAsync(Guid threadId);
    Task<List<ThreadSummaryDto>> GetThreadsForUserAsync(string userId);
    Task<Guid?> ValidateCredentialsAsync(string email, string password);
    Task<UserModel?> GetUserAsync(Guid userId);
    Task<bool> CreateUserAsync(UserModel newUser);
    Task<bool> UpdateUserAsync(UpdateUserModel updateUser, Guid userId);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<ChatHistorySnapshot?> GetHistorySnapshotAsync(Guid threadId);
    Task UpsertHistorySnapshotAsync(ChatHistorySnapshot snapshot);
}
