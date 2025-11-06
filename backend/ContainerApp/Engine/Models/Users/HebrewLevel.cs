using System.Text.Json.Serialization;

namespace Engine.Models.Users;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HebrewLevel
{
    beginner,
    intermediate,
    advanced,
    fluent
}
