using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Accessor.Models.Achievements;

[Table("Achievements")]
public class AchievementModel
{
    [Key]
    public Guid AchievementId { get; set; }

    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required AchievementType Type { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required PracticeFeature Feature { get; set; }

    public required int TargetCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
