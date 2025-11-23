using System.ComponentModel.DataAnnotations;

namespace Manager.Models.Achievements;

public class TrackProgressRequest
{
    [Required]
    public required Guid UserId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Feature { get; set; }

    [Range(1, 1000)]
    public int IncrementBy { get; set; } = 1;
}
