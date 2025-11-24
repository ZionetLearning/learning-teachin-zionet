using System.Text.Json.Serialization;
using Manager.Models.Meetings;

namespace Manager.Services.Clients.Accessor.Models.Meetings;

/// <summary>
/// Request model for creating a meeting (sent to Accessor service)
/// </summary>
public sealed record CreateMeetingAccessorRequest
{
    public required IReadOnlyList<AttendeeAccessorDto> Attendees { get; init; }

    public required DateTimeOffset StartTimeUtc { get; init; }

    public required int DurationMinutes { get; init; }

    public string? Description { get; init; }
    public required Guid CreatedByUserId { get; init; }
}

/// <summary>
/// Simplified attendee DTO for Accessor communication
/// </summary>
public sealed record AttendeeAccessorDto
{
    public required Guid UserId { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required AttendeeRole Role { get; init; }
}
