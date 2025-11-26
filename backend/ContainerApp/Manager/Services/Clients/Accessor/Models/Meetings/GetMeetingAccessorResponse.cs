using System.Text.Json.Serialization;
using Manager.Models.Meetings;

namespace Manager.Services.Clients.Accessor.Models.Meetings;

/// <summary>
/// Response model received from Accessor service for a single meeting
/// </summary>
public sealed record GetMeetingAccessorResponse
{
    public required Guid Id { get; init; }

    public required IReadOnlyList<MeetingAttendeeAccessorDto> Attendees { get; init; }

    public required DateTimeOffset StartTimeUtc { get; init; }

    public required int DurationMinutes { get; init; }

    public string? Description { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required MeetingStatus Status { get; init; }

    public required string GroupCallId { get; init; }

    public required DateTimeOffset CreatedOn { get; init; }

    public required Guid CreatedByUserId { get; init; }
}
