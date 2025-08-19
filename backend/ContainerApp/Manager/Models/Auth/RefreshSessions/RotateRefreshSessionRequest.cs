using System.ComponentModel.DataAnnotations;

namespace Manager.Models.Auth.RefreshSessions;

public class RotateRefreshSessionRequest
{
    [Required]
    [MinLength(64)]
    public string NewRefreshTokenHash { get; set; } = null!;
    public DateTimeOffset NewExpiresAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
}
