using Manager.Models.Games;
using Manager.Models.UserGameConfiguration;

namespace Manager.Services.Clients.Accessor.Models.UserGameConfiguration;

/// <summary>
/// Request model sent to Accessor service for saving game configuration
/// </summary>
public sealed record SaveGameConfigAccessorRequest
{
    public required Guid UserId { get; init; }
    public required GameName GameName { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required bool Nikud { get; init; }
    public required int NumberOfSentences { get; init; }
}
