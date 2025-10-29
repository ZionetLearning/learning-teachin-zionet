using System.Text.Json.Serialization;

namespace Accessor.Models.Meetings;

public class MeetingAttendee
{
    [JsonPropertyName("userId")]
    public required Guid UserId { get; set; }

    [JsonPropertyName("role")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required AttendeeRole Role { get; set; }
}
