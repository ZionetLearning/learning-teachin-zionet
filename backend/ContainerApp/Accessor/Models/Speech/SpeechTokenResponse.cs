namespace Accessor.Models.Speech;

public record SpeechTokenResponse
{
    public required string Token { get; init; }
    public required string Region { get; init; }
}