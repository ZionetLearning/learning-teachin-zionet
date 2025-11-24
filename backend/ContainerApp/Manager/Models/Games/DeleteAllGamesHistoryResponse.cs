namespace Manager.Models.Games;

/// <summary>
/// Response model for deleting all games history
/// </summary>
public sealed record DeleteAllGamesHistoryResponse
{
    public required string Message { get; init; }
}
