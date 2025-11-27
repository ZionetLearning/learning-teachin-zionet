namespace Manager.Services.Clients.Accessor.Models.WordCards;

/// <summary>
/// Request model for updating learned status (sent to Accessor service)
/// </summary>
public sealed record UpdateLearnedStatusAccessorRequest
{
    public required Guid UserId { get; init; }
    public required Guid CardId { get; init; }
    public required bool IsLearned { get; init; }
}
