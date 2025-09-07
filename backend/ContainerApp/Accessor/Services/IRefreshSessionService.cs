using Accessor.Models.RefreshSessions;

namespace Accessor.Services;

public interface IRefreshSessionService
{
    Task CreateSessionAsync(RefreshSessionRequest request, CancellationToken cancellationToken);
    Task<RefreshSessionDto?> FindByRefreshHashAsync(string refreshTokenHash, CancellationToken cancellationToken);
    Task RotateSessionAsync(Guid sessionId, RotateRefreshSessionRequest request, CancellationToken cancellationToken);
    Task DeleteSessionAsync(Guid sessionId, CancellationToken cancellationToken);
    Task DeleteAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken);
}
