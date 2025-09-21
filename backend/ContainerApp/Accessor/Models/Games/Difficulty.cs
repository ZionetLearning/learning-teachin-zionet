using System.Text.Json.Serialization;

namespace Accessor.Models.Games;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Difficulty
{
    Easy,
    Medium,
    Hard
}