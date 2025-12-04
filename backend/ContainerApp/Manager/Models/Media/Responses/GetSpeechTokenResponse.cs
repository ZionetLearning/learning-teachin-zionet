namespace Manager.Models.Media.Responses;

/// <summary>
/// Response model for getting speech token
/// </summary>
public sealed record GetSpeechTokenResponse
{
    public required string Token { get; init; }
    public required string Region { get; init; }
}
