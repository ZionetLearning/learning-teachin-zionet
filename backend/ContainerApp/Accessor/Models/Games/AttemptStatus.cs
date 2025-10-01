using System.Text.Json.Serialization;

namespace Accessor.Models.Games;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AttemptStatus
{
    Pending,   // just generated
    Success,   // answered correctly
    Failure    // answered incorrectly
}