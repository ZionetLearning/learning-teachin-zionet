using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Manager.Models.Meetings.Requests;

/// <summary>
/// Request model from frontend for updating a meeting
/// </summary>
public sealed record UpdateMeetingRequest
{
    [JsonPropertyName("attendees")]
    public IReadOnlyList<MeetingAttendee>? Attendees { get; init; }

    [JsonPropertyName("startTimeUtc")]
    public DateTimeOffset? StartTimeUtc { get; init; }

    [JsonPropertyName("durationMinutes")]
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 minute and 24 hours (1440 minutes)")]
    public int? DurationMinutes { get; init; }

    [JsonPropertyName("description")]
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; init; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MeetingStatus? Status { get; init; }
}
