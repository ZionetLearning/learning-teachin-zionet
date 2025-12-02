namespace Manager.Services.Clients.Accessor.Models.Media;

/// <summary>
/// Response model received from Accessor service for speech token
/// </summary>
public sealed record GetSpeechTokenAccessorResponse
{
    public required string Token { get; init; }
    public required string Region { get; init; }
}
