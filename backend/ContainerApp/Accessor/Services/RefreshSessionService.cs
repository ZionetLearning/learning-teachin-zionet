using System.Net;
using Accessor.DB;
using Accessor.Models;
using Accessor.Models.RefreshSessions;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class RefreshSessionService : IRefreshSessionService
{
    private readonly ILogger<RefreshSessionService> _logger;
    private readonly AccessorDbContext _dbContext;

    public RefreshSessionService(
        ILogger<RefreshSessionService> logger,
        AccessorDbContext dbContext
    )
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task CreateSessionAsync(RefreshSessionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating refresh session for user {UserId}", request.UserId);

            if (!IPAddress.TryParse(request.IP, out var ipAddress))
            {
                _logger.LogWarning("Invalid IP address provided: {IP}", request.IP);
                throw new ArgumentException("Invalid IP address", nameof(request));
            }

            var session = new RefreshSessionsRecord
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                RefreshTokenHash = request.RefreshTokenHash,
                DeviceFingerprintHash = request.DeviceFingerprintHash,
                IP = ipAddress,
                UserAgent = request.UserAgent
            };
            _dbContext.RefreshSessions.Add(session);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Refresh session {SessionId} created", session.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create refresh session for user {UserId}", request.UserId);
            throw new InvalidOperationException("Could not create refresh session", ex);
        }
    }

    public async Task<RefreshSessionDto?> FindByRefreshHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Looking up refresh session by token hash");

            var session = await _dbContext.RefreshSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, cancellationToken);

            if (session == null)
            {
                _logger.LogWarning("No session found for given refresh token hash");
                return null;
            }

            return new RefreshSessionDto
            {
                Id = session.Id,
                UserId = session.UserId,
                ExpiresAt = session.ExpiresAt,
                DeviceFingerprintHash = session.DeviceFingerprintHash,
                IP = session.IP.ToString(),
                UserAgent = session.UserAgent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding refresh session by token hash");
            throw new InvalidOperationException("Could not find refresh session", ex);
        }
    }

    public async Task RotateSessionAsync(Guid sessionId, RotateRefreshSessionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Rotating refresh session {SessionId}", sessionId);

            var session = await _dbContext.RefreshSessions.FindAsync([sessionId], cancellationToken);
            if (session == null)
            {
                _logger.LogWarning("Session {SessionId} not found", sessionId);
                return;
            }

            session.RefreshTokenHash = request.NewRefreshTokenHash;
            session.ExpiresAt = request.NewExpiresAt;
            session.LastSeenAt = request.LastSeenAt;
            session.IssuedAt = request.IssuedAt;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rotate refresh session {SessionId}", sessionId);
            throw new InvalidOperationException("Could not rotate refresh session", ex);
        }
    }

    public async Task DeleteSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting refresh session {SessionId}", sessionId);

            var session = await _dbContext.RefreshSessions.FindAsync(new object[] { sessionId }, cancellationToken);
            if (session == null)
            {
                _logger.LogWarning("Session {SessionId} not found", sessionId);
                return;
            }

            _dbContext.RefreshSessions.Remove(session);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete refresh session {SessionId}", sessionId);
            throw new InvalidOperationException("Could not delete refresh session", ex);
        }
    }

    // for now, we dont use it , maybe in the future
    public async Task DeleteAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting all refresh sessions for user {UserId}", userId);

            var sessions = await _dbContext.RefreshSessions
                .Where(s => s.UserId == userId)
                .ToListAsync(cancellationToken);

            if (sessions.Count == 0)
            {
                _logger.LogInformation("No sessions found for user {UserId}", userId);
                return;
            }

            _dbContext.RefreshSessions.RemoveRange(sessions);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete all refresh sessions for user {UserId}", userId);
            throw new InvalidOperationException("Could not delete user refresh sessions", ex);
        }
    }
}
