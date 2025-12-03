namespace Manager.Models.Summaries;

public sealed record DateRangePeriod
{
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
}
