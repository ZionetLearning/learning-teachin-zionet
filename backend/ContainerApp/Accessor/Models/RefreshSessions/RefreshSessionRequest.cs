using System.ComponentModel.DataAnnotations;

namespace Accessor.Models.RefreshSessions;

public class RefreshSessionRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MinLength(64)] // Assuming a SHA-256 or similar hash
    public string RefreshTokenHash { get; set; } = null!;

    [Required]
    [MinLength(64)] // Fingerprint hash as well
    public string DeviceFingerprintHash { get; set; } = null!;

    [Required]
    public DateTimeOffset ExpiresAt { get; set; }

    [Required]
    public string IP { get; set; } = null!;

    [Required]
    [StringLength(512)]
    public string UserAgent { get; set; } = null!;
}
