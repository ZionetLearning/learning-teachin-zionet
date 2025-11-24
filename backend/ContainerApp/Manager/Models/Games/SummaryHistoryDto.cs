namespace Manager.Models.Games;

public sealed record SummaryHistoryDto
{
    public required string GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required int AttemptsCount { get; init; }
    public required int TotalSuccesses { get; init; }
    public required int TotalFailures { get; init; }
}
