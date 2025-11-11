using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Manager.Models.Meetings;

public sealed record UpdateMeetingRequest
{
    [JsonPropertyName("attendees")]
    public List<MeetingAttendee>? Attendees { get; set; }

    [JsonPropertyName("startTimeUtc")]
    public DateTimeOffset? StartTimeUtc { get; set; }

    [JsonPropertyName("durationMinutes")]
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 minute and 24 hours (1440 minutes)")]
    public int? DurationMinutes { get; set; }

    [JsonPropertyName("description")]
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [JsonPropertyName("status")]
    public MeetingStatus? Status { get; set; }
}
