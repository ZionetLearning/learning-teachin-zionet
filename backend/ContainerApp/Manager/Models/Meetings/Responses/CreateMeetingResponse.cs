using System.Text.Json.Serialization;

namespace Manager.Models.Meetings.Responses;

/// <summary>
/// Response model after creating a meeting (sent to frontend)
/// </summary>
public sealed record CreateMeetingResponse
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("attendees")]
    public required IReadOnlyList<MeetingAttendee> Attendees { get; init; }

    [JsonPropertyName("startTimeUtc")]
    public required DateTimeOffset StartTimeUtc { get; init; }

    [JsonPropertyName("durationMinutes")]
    public required int DurationMinutes { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required MeetingStatus Status { get; init; }

    [JsonPropertyName("groupCallId")]
    public required string GroupCallId { get; init; }

    [JsonPropertyName("createdOn")]
    public required DateTimeOffset CreatedOn { get; init; }

    [JsonPropertyName("createdByUserId")]
    public required Guid CreatedByUserId { get; init; }
}
