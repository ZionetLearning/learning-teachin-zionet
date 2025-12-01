namespace Manager.Models.Summaries;

public sealed record GetPeriodWordCardsResponse
{
    public required WordCardsSummary Summary { get; init; }
    public List<WordCardInfo> RecentLearned { get; init; } = new();
    public List<WordCardInfo> NewCards { get; init; } = new();
}

public sealed record WordCardsSummary
{
    public int TotalCards { get; init; }
    public int NewInPeriod { get; init; }
    public int LearnedInPeriod { get; init; }
    public int TotalLearned { get; init; }
}

public sealed record WordCardInfo
{
    public required Guid CardId { get; init; }
    public required string Hebrew { get; init; }
    public required string English { get; init; }
    public required DateTime Timestamp { get; init; }
}
