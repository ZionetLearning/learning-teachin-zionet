using Manager.Models.Games;

namespace Manager.Models.UserGameConfiguration.Requests;

/// <summary>
/// Request model for saving game configuration
/// </summary>
public sealed record SaveGameConfigRequest
{
    public required GameName GameName { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required bool Nikud { get; init; }
    public required int NumberOfSentences { get; init; }
}
