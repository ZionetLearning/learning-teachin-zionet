using Manager.Models.Games;

namespace Manager.Models.UserGameConfiguration.Responses;

/// <summary>
/// Response model for getting game configuration
/// </summary>
public sealed record GetGameConfigResponse
{
    public required Guid UserId { get; init; }
    public required GameName GameName { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required bool Nikud { get; init; }
    public required int NumberOfSentences { get; init; }
}
