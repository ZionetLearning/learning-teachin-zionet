using System.Text.Json.Serialization;

namespace Manager.Models.Meetings.Responses;

/// <summary>
/// Response model for getting meetings for a user (sent to frontend)
/// </summary>
public sealed record GetUserMeetingsResponse
{
    [JsonPropertyName("meetings")]
    public required IReadOnlyList<GetMeetingResponse> Meetings { get; init; }
}
