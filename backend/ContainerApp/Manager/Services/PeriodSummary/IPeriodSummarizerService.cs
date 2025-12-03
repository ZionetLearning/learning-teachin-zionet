using Manager.Models.Summaries.Responses;

namespace Manager.Services.PeriodSummary;

public interface IPeriodSummarizerService
{
    Task<GetPeriodOverviewResponse> GetPeriodOverviewAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<GetGamePracticeSummaryResponse> GetPeriodGamePracticeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<GetPeriodWordCardsResponse> GetPeriodWordCardsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<GetPeriodAchievementsResponse> GetPeriodAchievementsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
}
