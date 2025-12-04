using Manager.Models.Games;
using Manager.Models.UserGameConfiguration;

namespace Manager.Services.Clients.Accessor.Models.UserGameConfiguration;

/// <summary>
/// Response model received from Accessor service for game configuration
/// </summary>
public sealed record GetGameConfigAccessorResponse
{
    public required Guid UserId { get; init; }
    public required GameName GameName { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required bool Nikud { get; init; }
    public required int NumberOfSentences { get; init; }
}
