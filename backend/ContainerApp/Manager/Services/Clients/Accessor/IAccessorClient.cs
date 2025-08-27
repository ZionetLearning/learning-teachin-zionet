using Manager.Models;
using Manager.Models.Auth;
using Manager.Models.Auth.RefreshSessions;
using Manager.Models.Chat;
using Manager.Models.Users;

namespace Manager.Services.Clients.Accessor;

public interface IAccessorClient
{
    Task<bool> UpdateTaskName(int id, string newTaskName);
    Task<bool> DeleteTask(int id);
    Task<(bool success, string message)> PostTaskAsync(TaskModel task);
    Task<TaskModel?> GetTaskAsync(int id);
    Task<IReadOnlyList<ChatMessage>> GetChatHistoryAsync(Guid threadId, CancellationToken ct = default);
    Task<ChatMessage?> StoreMessageAsync(ChatMessage msg, CancellationToken ct = default);
    Task<IReadOnlyList<ChatThread>> GetThreadsForUserAsync(string userId, CancellationToken ct = default);
    Task<UserModel?> GetUserAsync(Guid userId);
    Task<bool> CreateUserAsync(UserModel user);
    Task<bool> UpdateUserAsync(UpdateUserModel user, Guid userId);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<IEnumerable<UserData>> GetAllUsersAsync(CancellationToken ct = default);
    Task<StatsSnapshot?> GetStatsSnapshotAsync(CancellationToken ct = default);
    Task<Guid?> LoginUserAsync(LoginRequest loginRequest, CancellationToken ct = default);
    Task SaveSessionDBAsync(RefreshSessionRequest session, CancellationToken ct = default);
    Task<RefreshSessionDto> GetSessionAsync(string oldHash, CancellationToken ct = default);
    Task UpdateSessionDBAsync(Guid sessionId, RotateRefreshSessionRequest rotatePayload, CancellationToken ct);
    Task DeleteSessionDBAsync(Guid sessionId, CancellationToken ct);

}
