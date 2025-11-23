namespace Manager.Models.WordCards.Responses;

/// <summary>
/// Response model after updating learned status (sent to frontend)
/// </summary>
public sealed record UpdateLearnedStatusResponse
{
    public required Guid CardId { get; init; }
    public required bool IsLearned { get; init; }
}
