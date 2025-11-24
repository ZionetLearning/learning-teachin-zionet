using System.Text.Json.Serialization;

namespace Manager.Models.Meetings;

public sealed record MeetingAttendee
{
    [JsonPropertyName("userId")]
    public required Guid UserId { get; init; }

    [JsonPropertyName("role")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required AttendeeRole Role { get; init; }
}
