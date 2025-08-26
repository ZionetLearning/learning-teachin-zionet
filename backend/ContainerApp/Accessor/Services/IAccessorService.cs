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
    Task CreateChatAsync(ChatHistorySnapshot chat);
    Task<List<ChatSummaryDto>> GetChatsForUserAsync(Guid userId);
    Task<Guid?> ValidateCredentialsAsync(string email, string password);
    Task<UserModel?> GetUserAsync(Guid userId);
    Task<bool> CreateUserAsync(UserModel newUser);
    Task<bool> UpdateUserAsync(UpdateUserModel updateUser, Guid userId);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<IEnumerable<UserData>> GetAllUsersAsync();
    Task<ChatHistorySnapshot?> GetHistorySnapshotAsync(Guid threadId);
    Task UpsertHistorySnapshotAsync(ChatHistorySnapshot snapshot);
    Task<StatsSnapshot> ComputeStatsAsync(CancellationToken ct = default);
}
