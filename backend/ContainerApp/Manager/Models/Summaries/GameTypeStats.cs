namespace Manager.Models.Summaries;

public sealed record GameTypeStats
{
    public required string GameType { get; init; }
    public int Attempts { get; init; }
    public decimal SuccessRate { get; init; }
    public decimal AverageAccuracy { get; init; }
    public int MistakesCount { get; init; }
}
