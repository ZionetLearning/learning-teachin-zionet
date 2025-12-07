namespace Manager.Models.Summaries.Responses;

public sealed record GetPeriodOverviewResponse
{
    public required DateRangePeriod Period { get; init; }
    public required PeriodOverviewSummary Summary { get; init; }
}
