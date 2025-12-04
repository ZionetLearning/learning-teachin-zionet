namespace Manager.Models.Summaries;

public sealed record WordCardsSummary
{
    public int TotalCards { get; init; }
    public int NewInPeriod { get; init; }
    public int LearnedInPeriod { get; init; }
    public int TotalLearned { get; init; }
}
