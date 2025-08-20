using System.ComponentModel.DataAnnotations;

namespace Manager.Models.Auth.RefreshSessions;

public class RefreshSessionRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required, MinLength(64)]
    public string RefreshTokenHash { get; set; } = null!;

    public string? DeviceFingerprintHash { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    [Required]
    public string IP { get; set; } = null!;

    [Required]
    public string UserAgent { get; set; } = null!;
}
