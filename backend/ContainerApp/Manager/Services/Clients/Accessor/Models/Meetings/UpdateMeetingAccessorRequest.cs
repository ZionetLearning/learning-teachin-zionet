using System.Text.Json.Serialization;
using Manager.Models.Meetings;

namespace Manager.Services.Clients.Accessor.Models.Meetings;

/// <summary>
/// Request model for updating a meeting (sent to Accessor service)
/// </summary>
public sealed record UpdateMeetingAccessorRequest
{
    [JsonPropertyName("attendees")]
    public IReadOnlyList<AttendeeAccessorDto>? Attendees { get; init; }

    [JsonPropertyName("startTimeUtc")]
    public DateTimeOffset? StartTimeUtc { get; init; }

    [JsonPropertyName("durationMinutes")]
    public int? DurationMinutes { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MeetingStatus? Status { get; init; }
}
