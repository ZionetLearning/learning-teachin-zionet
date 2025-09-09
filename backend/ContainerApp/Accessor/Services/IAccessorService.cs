using Accessor.Models;
using Accessor.Models.Users;
using Accessor.Models.Auth;

namespace Accessor.Services;

public interface IAccessorService
{
    Task InitializeAsync();
    Task<(TaskModel Task, string ETag)?> GetTaskWithEtagAsync(int id);
    Task<TaskModel?> GetTaskByIdAsync(int id);
    Task CreateTaskAsync(TaskModel task);
    Task<bool> DeleteTaskAsync(int taskId);
    Task<UpdateTaskResult> UpdateTaskNameAsync(int taskId, string newName, string? ifMatch);
    Task CreateChatAsync(ChatHistorySnapshot chat);
    Task<List<ChatSummaryDto>> GetChatsForUserAsync(Guid userId);
    Task<AuthenticatedUser?> ValidateCredentialsAsync(string email, string password);
    Task<UserData?> GetUserAsync(Guid userId);
    Task<bool> CreateUserAsync(UserModel newUser);
    Task<bool> UpdateUserAsync(UpdateUserModel updateUser, Guid userId);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<IEnumerable<UserData>> GetAllUsersAsync();
    Task<ChatHistorySnapshot?> GetHistorySnapshotAsync(Guid threadId);
    Task UpsertHistorySnapshotAsync(ChatHistorySnapshot snapshot);
    Task<StatsSnapshot> ComputeStatsAsync(CancellationToken ct = default);
}
