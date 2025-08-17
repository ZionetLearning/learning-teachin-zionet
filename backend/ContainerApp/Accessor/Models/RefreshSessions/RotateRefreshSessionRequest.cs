using System.ComponentModel.DataAnnotations;

namespace Accessor.Models.RefreshSessions;

public class RotateRefreshSessionRequest
{
    [Required]
    [MinLength(64)]
    public string NewRefreshTokenHash { get; set; } = null!;

    [Required]
    public DateTimeOffset NewExpiresAt { get; set; }
}
