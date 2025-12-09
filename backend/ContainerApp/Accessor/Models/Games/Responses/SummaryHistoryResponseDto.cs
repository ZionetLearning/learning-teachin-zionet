using Accessor.Models.GameConfiguration;

namespace Accessor.Models.Games.Responses;

/// <summary>
/// DTO representing summary history in response
/// </summary>
public sealed record SummaryHistoryResponseDto
{
    public required GameName GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required int AttemptsCount { get; init; }
    public required int TotalSuccesses { get; init; }
    public required int TotalFailures { get; init; }
}

