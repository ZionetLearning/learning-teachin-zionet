using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Manager.Models.Meetings.Requests;

/// <summary>
/// Request model from frontend for creating a meeting
/// </summary>
public sealed record CreateMeetingRequest
{
    [Required]
    [JsonPropertyName("attendees")]
    public required IReadOnlyList<MeetingAttendee> Attendees { get; init; }

    [Required]
    [JsonPropertyName("startTimeUtc")]
    public required DateTimeOffset StartTimeUtc { get; init; }

    [Required]
    [JsonPropertyName("durationMinutes")]
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 minute and 24 hours (1440 minutes)")]
    public required int DurationMinutes { get; init; }

    [JsonPropertyName("description")]
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; init; }
}
