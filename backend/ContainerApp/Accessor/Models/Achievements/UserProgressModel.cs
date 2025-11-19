using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Accessor.Models.Achievements;

[Table("UserProgress")]
public class UserProgressModel
{
    [Key]
    public Guid UserProgressId { get; set; }

    public required Guid UserId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required PracticeFeature Feature { get; set; }

    public int Count { get; set; }
    public DateTime UpdatedAt { get; set; }
}
