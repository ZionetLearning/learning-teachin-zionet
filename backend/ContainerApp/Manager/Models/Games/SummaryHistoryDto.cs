using Manager.Models.UserGameConfiguration;

namespace Manager.Models.Games;

/// <summary>
/// Matches Accessor's SummaryHistoryResponseDto
/// </summary>
public sealed record SummaryHistoryDto
{
    public required GameName GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required int AttemptsCount { get; init; }
    public required int TotalSuccesses { get; init; }
    public required int TotalFailures { get; init; }
}
