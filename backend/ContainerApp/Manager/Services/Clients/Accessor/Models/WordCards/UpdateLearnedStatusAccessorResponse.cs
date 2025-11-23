namespace Manager.Services.Clients.Accessor.Models.WordCards;

/// <summary>
/// Response model received from Accessor service after updating learned status
/// </summary>
public sealed record UpdateLearnedStatusAccessorResponse
{
    public required Guid CardId { get; init; }

    public required bool IsLearned { get; init; }
}
