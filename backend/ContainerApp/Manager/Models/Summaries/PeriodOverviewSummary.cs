namespace Manager.Models.Summaries;

public sealed record PeriodOverviewSummary
{
    public int TotalAttempts { get; init; }
    public int WordsLearned { get; init; }
    public int AchievementsUnlocked { get; init; }
    public int PracticeDays { get; init; }
}
