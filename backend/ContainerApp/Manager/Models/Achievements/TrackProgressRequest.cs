using System.ComponentModel.DataAnnotations;

namespace Manager.Models.Achievements;

public sealed record TrackProgressRequest
{
    [Required]
    public required Guid UserId { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Feature { get; init; }

    [Range(1, 1000)]
    public int IncrementBy { get; init; } = 1;
}
