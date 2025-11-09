using System.Text.Json.Serialization;
using Manager.Models.Meetings;

namespace Manager.Services.Clients.Accessor.Models;

/// <summary>
/// Internal DTO for sending meeting creation requests to the Accessor service
/// </summary>
internal sealed record CreateMeetingAccessorRequest
{
    public required List<AttendeeAccessorDto> Attendees { get; set; }
    public required DateTimeOffset StartTimeUtc { get; set; }
    public required int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public required Guid CreatedByUserId { get; set; }
}

/// <summary>
/// Simplified attendee DTO for Accessor communication
/// </summary>
internal sealed record AttendeeAccessorDto
{
    public required Guid UserId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required AttendeeRole Role { get; set; }
}