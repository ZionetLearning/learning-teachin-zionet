using Manager.Models;
using Manager.Models.Auth;
using Manager.Models.Auth.RefreshSessions;
using Manager.Models.Chat;
using Manager.Models.Classes;
using Manager.Models.UserGameConfiguration;
using Manager.Models.WordCards;
using Manager.Services.Clients.Accessor.Models;

namespace Manager.Services.Clients.Accessor.Interfaces;

public interface IAccessorClient
{
    Task<int> CleanupRefreshSessionsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ChatSummary>> GetChatsForUserAsync(Guid userId, CancellationToken ct = default);
    Task<StatsSnapshot?> GetStatsSnapshotAsync(CancellationToken ct = default);
    Task<AuthenticatedUser?> LoginUserAsync(LoginRequest loginRequest, CancellationToken ct = default);
    Task SaveSessionDBAsync(RefreshSessionRequest session, CancellationToken ct = default);
    Task<RefreshSessionDto> GetSessionAsync(string oldHash, CancellationToken ct = default);
    Task UpdateSessionDBAsync(Guid sessionId, RotateRefreshSessionRequest rotatePayload, CancellationToken ct);
    Task DeleteSessionDBAsync(Guid sessionId, CancellationToken ct);
    Task<SpeechTokenResponse> GetSpeechTokenAsync(CancellationToken ct = default);

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
