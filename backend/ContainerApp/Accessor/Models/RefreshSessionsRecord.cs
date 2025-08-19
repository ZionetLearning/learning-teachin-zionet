using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace Accessor.Models;

[Table("refreshSessions")]
public class RefreshSessionsRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string RefreshTokenHash { get; set; } = null!;
    public string? DeviceFingerprintHash { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public IPAddress IP { get; set; } = null!;
    public string UserAgent { get; set; } = null!;
}
