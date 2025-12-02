namespace Manager.Services.Clients.Accessor.Models.WordCards;

/// <summary>
/// Response model received from Accessor service after creating a word card
/// </summary>
public sealed record CreateWordCardAccessorResponse
{
    public required Guid CardId { get; init; }

    public required string Hebrew { get; init; }

    public required string English { get; init; }

    public required bool IsLearned { get; init; }

    public string? Explanation { get; init; }
}
