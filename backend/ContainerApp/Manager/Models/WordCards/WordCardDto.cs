namespace Manager.Models.WordCards;

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
