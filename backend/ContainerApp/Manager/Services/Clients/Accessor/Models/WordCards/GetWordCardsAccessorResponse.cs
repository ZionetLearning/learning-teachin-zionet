namespace Manager.Services.Clients.Accessor.Models.WordCards;

/// <summary>
/// Response model received from Accessor service for getting word cards
/// </summary>
public sealed record GetWordCardsAccessorResponse
{
    public required IReadOnlyList<WordCardAccessorDto> WordCards { get; init; }
}

/// <summary>
/// Word card DTO received from Accessor
/// </summary>
public sealed record WordCardAccessorDto
{
    public required Guid CardId { get; init; }
    public required string Hebrew { get; init; }
    public required string English { get; init; }
    public required bool IsLearned { get; init; }
    public string? Explanation { get; init; }
}
