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
        _logger.LogInformation("Creating refresh session for user {UserId}", request.UserId);

        if (!IPAddress.TryParse(request.IP, out var ipAddress))
        {
            _logger.LogWarning("Invalid IP address provided: {IP}", request.IP);
            throw new ArgumentException("Invalid IP address", nameof(request));
        }

        var now = DateTimeOffset.UtcNow;

        var session = new RefreshSessionsRecord
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            RefreshTokenHash = request.RefreshTokenHash,
            DeviceFingerprintHash = request.DeviceFingerprintHash,
            IssuedAt = now,
            ExpiresAt = request.ExpiresAt,
            LastSeenAt = now,
            IP = ipAddress,
            UserAgent = request.UserAgent
        };
        _dbContext.RefreshSessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh session {SessionId} created", session.Id);
    }

    public async Task<RefreshSessionDto?> FindByRefreshHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
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
            IssuedAt = session.IssuedAt,
            ExpiresAt = session.ExpiresAt,
            LastSeenAt = session.LastSeenAt,
            IP = session.IP.ToString(),
            UserAgent = session.UserAgent
        };
    }

    public async Task RotateSessionAsync(Guid sessionId, RotateRefreshSessionRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rotating refresh session {SessionId}", sessionId);

        var session = await _dbContext.RefreshSessions.FindAsync(new object[] { sessionId }, cancellationToken);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found", sessionId);
            return;
        }

        session.RefreshTokenHash = request.NewRefreshTokenHash;
        session.ExpiresAt = request.NewExpiresAt;
        session.LastSeenAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteSessionAsync(Guid sessionId, CancellationToken cancellationToken)
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

    public async Task DeleteAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken)
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
}
