namespace Manager.Models.WordCards.Responses;

/// <summary>
/// Response model after creating a word card (sent to frontend)
/// </summary>
public sealed record CreateWordCardResponse
{
    public required Guid CardId { get; init; }
    public required string Hebrew { get; init; }
    public required string English { get; init; }
    public required bool IsLearned { get; init; }
    public string? Explanation { get; init; }
}
