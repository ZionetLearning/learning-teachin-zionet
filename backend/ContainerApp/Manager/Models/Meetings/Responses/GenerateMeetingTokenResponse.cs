using System.Text.Json.Serialization;

namespace Manager.Models.Meetings.Responses;

/// <summary>
/// Response model for generating meeting token (sent to frontend)
/// </summary>
public sealed record GenerateMeetingTokenResponse
{
    [JsonPropertyName("userId")]
    public required string UserId { get; init; }

    [JsonPropertyName("token")]
    public required string Token { get; init; }

    [JsonPropertyName("expiresOn")]
    public required DateTimeOffset ExpiresOn { get; init; }

    [JsonPropertyName("groupId")]
    public required string GroupId { get; init; }
}
