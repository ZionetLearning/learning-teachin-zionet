namespace Manager.Models.Summaries;

public sealed record GetPeriodOverviewResponse
{
    public required DateRangePeriod Period { get; init; }
    public required PeriodOverviewSummary Summary { get; init; }
}
