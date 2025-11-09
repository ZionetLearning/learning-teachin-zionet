using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Manager.Models.Meetings;

public sealed record CreateMeetingRequest
{
    [Required]
    [JsonPropertyName("attendees")]
    public required List<MeetingAttendee> Attendees { get; set; }

    [Required]
    [JsonPropertyName("startTimeUtc")]
    public DateTimeOffset StartTimeUtc { get; set; }

    [Required]
    [JsonPropertyName("durationMinutes")]
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 minute and 24 hours (1440 minutes)")]
    public required int DurationMinutes { get; set; }

    [JsonPropertyName("description")]
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}
