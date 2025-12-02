using Manager.Models;
using Manager.Models.Auth;
using Manager.Models.Auth.RefreshSessions;
using Manager.Models.Chat;
using Manager.Models.UserGameConfiguration;
using Manager.Services.Clients.Accessor.Models.Media;
using Manager.Services.Clients.Accessor.Models.UserGameConfiguration;

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
    Task<GetSpeechTokenAccessorResponse> GetSpeechTokenAsync(CancellationToken ct = default);
    Task<GetGameConfigAccessorResponse> GetUserGameConfigAsync(Guid userId, GameName gameName, CancellationToken ct = default);
    Task SaveUserGameConfigAsync(SaveGameConfigAccessorRequest request, CancellationToken ct = default);
    Task DeleteUserGameConfigAsync(Guid userId, GameName gameName, CancellationToken ct = default);
}
