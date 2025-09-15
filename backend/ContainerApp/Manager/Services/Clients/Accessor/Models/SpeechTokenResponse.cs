namespace Manager.Services.Clients.Accessor.Models;

public record SpeechTokenResponse
{
    public required string Token { get; init; }
    public required string Region { get; init; }
}