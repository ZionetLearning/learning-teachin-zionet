using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accessor.Models;

[Table("refresh_sessions")]
public class RefreshSessionsRecord
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("refresh_token_hash")]
    [MaxLength(128)] // assuming SHA-512 or similar
    public string RefreshTokenHash { get; set; } = null!;

    [Required]
    [Column("device_fingerprint_hash")]
    [MaxLength(128)]
    public string DeviceFingerprintHash { get; set; } = null!;

    [Required]
    [Column("issued_at")]
    public DateTime IssuedAt { get; set; }

    [Required]
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Required]
    [Column("last_seen_at")]
    public DateTime LastSeenAt { get; set; }

    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    [Required]
    [Column("ip")]
    [MaxLength(45)] // supports IPv6
    public string IP { get; set; } = null!;

    [Required]
    [Column("user_agent")]
    [MaxLength(512)]
    public string UserAgent { get; set; } = null!;
}
