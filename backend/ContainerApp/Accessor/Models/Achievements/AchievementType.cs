using System.Text.Json.Serialization;

namespace Accessor.Models.Achievements;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AchievementType
{
    Count,
    Milestone
}
