namespace Manager.Models.Summaries;

public sealed record GamePracticeSummary
{
    public int TotalAttempts { get; init; }
    public decimal SuccessRate { get; init; }
    public decimal AverageAccuracy { get; init; }
}
