namespace Accessor.Models.Games;

public class SummaryHistoryDto
{
    public required string GameType { get; set; } = string.Empty;
    public required Difficulty Difficulty { get; set; }
    public required int AttemptsCount { get; set; }
    public required int TotalSuccesses { get; set; }
    public required int TotalFailures { get; set; }
}