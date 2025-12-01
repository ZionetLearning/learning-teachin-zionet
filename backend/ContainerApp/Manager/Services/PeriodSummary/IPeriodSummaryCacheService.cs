using Manager.Models.Summaries;
using Manager.Services.Clients.Accessor.Models.Games;
using Manager.Services.Clients.Accessor.Models.WordCards;
using Manager.Services.Clients.Accessor.Models.Achievements;

namespace Manager.Services.PeriodSummary;

public interface IPeriodSummaryCacheService
{
    Task<GetHistoryAccessorResponse?> GetCachedHistoryAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task CacheHistoryAsync(Guid userId, DateTime startDate, DateTime endDate, GetHistoryAccessorResponse data, CancellationToken ct = default);

    Task<GetWordCardsAccessorResponse?> GetCachedWordCardsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task CacheWordCardsAsync(Guid userId, DateTime startDate, DateTime endDate, GetWordCardsAccessorResponse data, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, DateTime>?> GetCachedAchievementsMapAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task CacheAchievementsMapAsync(Guid userId, DateTime startDate, DateTime endDate, IReadOnlyDictionary<Guid, DateTime> data, CancellationToken ct = default);

    Task<GetMistakesAccessorResponse?> GetCachedMistakesAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task CacheMistakesAsync(Guid userId, DateTime startDate, DateTime endDate, GetMistakesAccessorResponse data, CancellationToken ct = default);

    Task<IReadOnlyList<AchievementAccessorModel>?> GetCachedAllAchievementsAsync(CancellationToken ct = default);
    Task CacheAllAchievementsAsync(IReadOnlyList<AchievementAccessorModel> data, CancellationToken ct = default);

    Task<GetPeriodOverviewResponse?> GetCachedOverviewAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task CacheOverviewAsync(Guid userId, DateTime startDate, DateTime endDate, GetPeriodOverviewResponse data, CancellationToken ct = default);

    Task<GetGamePracticeSummaryResponse?> GetCachedGamePracticeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task CacheGamePracticeAsync(Guid userId, DateTime startDate, DateTime endDate, GetGamePracticeSummaryResponse data, CancellationToken ct = default);

    Task<GetPeriodWordCardsResponse?> GetCachedWordCardsSummaryAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task CacheWordCardsSummaryAsync(Guid userId, DateTime startDate, DateTime endDate, GetPeriodWordCardsResponse data, CancellationToken ct = default);

    Task<GetPeriodAchievementsResponse?> GetCachedAchievementsSummaryAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task CacheAchievementsSummaryAsync(Guid userId, DateTime startDate, DateTime endDate, GetPeriodAchievementsResponse data, CancellationToken ct = default);
}
