using Manager.Models;
using Manager.Models.Auth;
using Manager.Models.Auth.RefreshSessions;
using Manager.Models.Chat;
using Manager.Models.Classes;
using Manager.Models.UserGameConfiguration;
using Manager.Models.Users;
using Manager.Models.WordCards;
using Manager.Services.Clients.Accessor.Models;

namespace Manager.Services.Clients.Accessor;

public interface IAccessorClient
{
    Task<(TaskModel? Task, string? ETag)> GetTaskWithEtagAsync(int id, CancellationToken ct = default);
    Task<UpdateTaskNameResult> UpdateTaskNameAsync(int id, string newTaskName, string ifMatch, CancellationToken ct = default);
    Task<bool> UpdateTaskName(int id, string newTaskName);
    Task<bool> DeleteTask(int id);
    Task<(bool success, string message)> PostTaskAsync(TaskModel task);
    Task<TaskModel?> GetTaskAsync(int id);
    Task<int> CleanupRefreshSessionsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ChatSummary>> GetChatsForUserAsync(Guid userId, CancellationToken ct = default);
    Task<UserData?> GetUserAsync(Guid userId);
    Task<bool> CreateUserAsync(UserModel user);
    Task<bool> UpdateUserAsync(UpdateUserModel user, Guid userId);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<IEnumerable<UserData>> GetAllUsersAsync(CancellationToken ct = default);
    Task<StatsSnapshot?> GetStatsSnapshotAsync(CancellationToken ct = default);
    Task<AuthenticatedUser?> LoginUserAsync(LoginRequest loginRequest, CancellationToken ct = default);
    Task SaveSessionDBAsync(RefreshSessionRequest session, CancellationToken ct = default);
    Task<RefreshSessionDto> GetSessionAsync(string oldHash, CancellationToken ct = default);
    Task UpdateSessionDBAsync(Guid sessionId, RotateRefreshSessionRequest rotatePayload, CancellationToken ct);
    Task DeleteSessionDBAsync(Guid sessionId, CancellationToken ct);
    Task<IEnumerable<UserData>> GetUsersForCallerAsync(CallerContextDto context, CancellationToken ct = default);
    Task<bool> AssignStudentToTeacherAsync(TeacherStudentMapDto map, CancellationToken ct = default);
    Task<bool> UnassignStudentFromTeacherAsync(TeacherStudentMapDto map, CancellationToken ct = default);
    Task<IEnumerable<UserData>> GetStudentsForTeacherAsync(Guid teacherId, CancellationToken ct = default);
    Task<IEnumerable<UserData>> GetTeachersForStudentAsync(Guid studentId, CancellationToken ct = default);
    Task<SpeechTokenResponse> GetSpeechTokenAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TaskSummaryDto>> GetTaskSummariesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<WordCard>> GetWordCardsAsync(Guid userId, CancellationToken ct);
    Task<WordCard> CreateWordCardAsync(Guid userId, CreateWordCardRequest request, CancellationToken ct);
    Task<WordCardLearnedStatus> UpdateLearnedStatusAsync(Guid userId, LearnedStatus request, CancellationToken ct);
    Task<ClassDto?> GetClassAsync(Guid classId, CancellationToken ct = default);
    Task<List<ClassDto?>?> GetMyClassesAsync(Guid callerId, CancellationToken ct = default);
    Task<List<ClassDto?>?> GetAllClassesAsync(CancellationToken ct = default);
    Task<Class?> CreateClassAsync(CreateClassRequest request, CancellationToken ct = default);
    Task<bool> AddMembersToClassAsync(Guid classId, AddMembersRequest request, CancellationToken ct = default);
    Task<bool> RemoveMembersFromClassAsync(Guid classId, RemoveMembersRequest request, CancellationToken ct = default);
    Task<bool> DeleteClassAsync(Guid classId, CancellationToken ct);
    Task<UserGameConfig> GetUserGameConfigAsync(Guid userId, GameName gameName, CancellationToken ct = default);
    Task SaveUserGameConfigAsync(Guid userId, UserNewGameConfig gameName, CancellationToken ct = default);
    Task DeleteUserGameConfigAsync(Guid userId, GameName gameName, CancellationToken ct = default);
}
