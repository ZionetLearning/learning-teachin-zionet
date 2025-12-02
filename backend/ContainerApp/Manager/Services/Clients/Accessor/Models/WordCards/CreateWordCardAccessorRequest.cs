namespace Manager.Services.Clients.Accessor.Models.WordCards;

/// <summary>
/// Request model for creating a word card (sent to Accessor service)
/// </summary>
public sealed record CreateWordCardAccessorRequest
{
    public required Guid UserId { get; init; }
    public required string Hebrew { get; init; }
    public required string English { get; init; }
    public string? Explanation { get; init; }
}
