namespace Manager.Models.WordCards.Responses;

/// <summary>
/// Response model for getting word cards (sent to frontend)
/// </summary>
public sealed record GetWordCardsResponse
{
    public required IReadOnlyList<WordCardDto> WordCards { get; init; }
}

/// <summary>
/// Word card data transfer object
/// </summary>
public sealed record WordCardDto
{
    public required Guid CardId { get; init; }
    public required string Hebrew { get; init; }
    public required string English { get; init; }
    public required bool IsLearned { get; init; }
    public string? Explanation { get; init; }
}
