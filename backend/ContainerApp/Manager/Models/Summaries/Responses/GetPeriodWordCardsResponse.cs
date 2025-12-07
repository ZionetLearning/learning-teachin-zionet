namespace Manager.Models.Summaries.Responses;

public sealed record GetPeriodWordCardsResponse
{
    public required WordCardsSummary Summary { get; init; }
    public List<WordCardInfo> RecentLearned { get; init; } = new();
    public List<WordCardInfo> NewCards { get; init; } = new();
}
