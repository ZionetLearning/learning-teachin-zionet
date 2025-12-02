namespace Manager.Models.WordCards.Requests;

/// <summary>
/// Request model for updating word card learned status (from frontend)
/// </summary>
public sealed record UpdateLearnedStatusRequest
{
    public required Guid CardId { get; init; }

    public required bool IsLearned { get; init; }
}
