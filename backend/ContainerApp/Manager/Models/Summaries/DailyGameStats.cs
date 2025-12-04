namespace Manager.Models.Summaries;

public sealed record DailyGameStats
{
    public required DateTime Date { get; init; }
    public int Attempts { get; init; }
    public decimal SuccessRate { get; init; }
}
