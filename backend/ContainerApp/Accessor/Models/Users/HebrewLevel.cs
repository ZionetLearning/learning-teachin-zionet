using System.Text.Json.Serialization;

namespace Accessor.Models.Users;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HebrewLevel
{
    beginner,
    intermediate,
    advanced,
    fluent
}
