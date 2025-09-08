using System.Text.Json.Serialization;

namespace Manager.Models.Users;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HebrewLevel
{
    beginner,
    intermediate,
    advanced,
    fluent
}
