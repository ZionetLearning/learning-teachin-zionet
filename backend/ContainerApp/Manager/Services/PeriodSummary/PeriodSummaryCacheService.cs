using Dapr.Client;
using Manager.Constants;
using Manager.Models.Summaries;
using Manager.Services.Clients.Accessor.Models.Games;
using Manager.Services.Clients.Accessor.Models.WordCards;
using Manager.Services.Clients.Accessor.Models.Achievements;

namespace Manager.Services.PeriodSummary;

public class PeriodSummaryCacheService : IPeriodSummaryCacheService
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<PeriodSummaryCacheService> _logger;

    private static readonly IReadOnlyDictionary<string, string> DefaultTtlMetadata = new Dictionary<string, string>
    {
        ["ttlInSeconds"] = PeriodSummaryCacheKeys.DefaultTtlSeconds.ToString()
    };

    public PeriodSummaryCacheService(DaprClient daprClient, ILogger<PeriodSummaryCacheService> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task<GetHistoryAccessorResponse?> GetCachedHistoryAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.History(userId, startDate, endDate);
        return await GetCachedDataAsync<GetHistoryAccessorResponse>("history", key, ct);
    }

    public async Task CacheHistoryAsync(Guid userId, DateTime startDate, DateTime endDate, GetHistoryAccessorResponse data, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.History(userId, startDate, endDate);
        await CacheDataAsync("history", key, data, DefaultTtlMetadata, ct);
    }

    public async Task<GetWordCardsAccessorResponse?> GetCachedWordCardsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.WordCards(userId, startDate, endDate);
        return await GetCachedDataAsync<GetWordCardsAccessorResponse>("word-cards", key, ct);
    }

    public async Task CacheWordCardsAsync(Guid userId, DateTime startDate, DateTime endDate, GetWordCardsAccessorResponse data, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.WordCards(userId, startDate, endDate);
        await CacheDataAsync("word-cards", key, data, DefaultTtlMetadata, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, DateTime>?> GetCachedAchievementsMapAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.AchievementsMap(userId, startDate, endDate);
        return await GetCachedDataAsync<IReadOnlyDictionary<Guid, DateTime>>("achievements-map", key, ct);
    }

    public async Task CacheAchievementsMapAsync(Guid userId, DateTime startDate, DateTime endDate, IReadOnlyDictionary<Guid, DateTime> data, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.AchievementsMap(userId, startDate, endDate);
        await CacheDataAsync("achievements-map", key, data, DefaultTtlMetadata, ct);
    }

    public async Task<GetMistakesAccessorResponse?> GetCachedMistakesAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.Mistakes(userId, startDate, endDate);
        return await GetCachedDataAsync<GetMistakesAccessorResponse>("mistakes", key, ct);
    }

    public async Task CacheMistakesAsync(Guid userId, DateTime startDate, DateTime endDate, GetMistakesAccessorResponse data, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.Mistakes(userId, startDate, endDate);
        await CacheDataAsync("mistakes", key, data, DefaultTtlMetadata, ct);
    }

    public async Task<IReadOnlyList<AchievementAccessorModel>?> GetCachedAllAchievementsAsync(CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.AllAchievements();
        return await GetCachedDataAsync<IReadOnlyList<AchievementAccessorModel>>("all-achievements", key, ct);
    }

    public async Task CacheAllAchievementsAsync(IReadOnlyList<AchievementAccessorModel> data, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.AllAchievements();
        await CacheDataAsync("all-achievements", key, data, DefaultTtlMetadata, ct);
    }

    public async Task<GetPeriodOverviewResponse?> GetCachedOverviewAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.Overview(userId, startDate, endDate);
        return await GetCachedDataAsync<GetPeriodOverviewResponse>("overview", key, ct);
    }

    public async Task CacheOverviewAsync(Guid userId, DateTime startDate, DateTime endDate, GetPeriodOverviewResponse data, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.Overview(userId, startDate, endDate);
        await CacheDataAsync("overview", key, data, DefaultTtlMetadata, ct);
    }

    public async Task<GetGamePracticeSummaryResponse?> GetCachedGamePracticeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.GamePractice(userId, startDate, endDate);
        return await GetCachedDataAsync<GetGamePracticeSummaryResponse>("game-practice", key, ct);
    }

    public async Task CacheGamePracticeAsync(Guid userId, DateTime startDate, DateTime endDate, GetGamePracticeSummaryResponse data, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.GamePractice(userId, startDate, endDate);
        await CacheDataAsync("game-practice", key, data, DefaultTtlMetadata, ct);
    }

    public async Task<GetPeriodWordCardsResponse?> GetCachedWordCardsSummaryAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.WordCardsSummary(userId, startDate, endDate);
        return await GetCachedDataAsync<GetPeriodWordCardsResponse>("word-cards-summary", key, ct);
    }

    public async Task CacheWordCardsSummaryAsync(Guid userId, DateTime startDate, DateTime endDate, GetPeriodWordCardsResponse data, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.WordCardsSummary(userId, startDate, endDate);
        await CacheDataAsync("word-cards-summary", key, data, DefaultTtlMetadata, ct);
    }

    public async Task<GetPeriodAchievementsResponse?> GetCachedAchievementsSummaryAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.AchievementsSummary(userId, startDate, endDate);
        return await GetCachedDataAsync<GetPeriodAchievementsResponse>("achievements-summary", key, ct);
    }

    public async Task CacheAchievementsSummaryAsync(Guid userId, DateTime startDate, DateTime endDate, GetPeriodAchievementsResponse data, CancellationToken ct = default)
    {
        var key = PeriodSummaryCacheKeys.AchievementsSummary(userId, startDate, endDate);
        await CacheDataAsync("achievements-summary", key, data, DefaultTtlMetadata, ct);
    }

    #region Helpers
    private async Task<T?> GetCachedDataAsync<T>(string dataType, string key, CancellationToken ct) where T : class
    {
        try
        {
            var cached = await _daprClient.GetStateAsync<T?>(
                AppIds.StateStore,
                key,
                cancellationToken: ct);

            if (cached != null)
            {
                _logger.LogInformation("Cache HIT for {DataType}. Key={Key}", dataType, key);
            }
            else
            {
                _logger.LogDebug("Cache MISS for {DataType}. Key={Key}", dataType, key);
            }

            return cached;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get cached {DataType}. Key={Key}", dataType, key);
            return null;
        }
    }

    private async Task CacheDataAsync<T>(string dataType, string key, T data, IReadOnlyDictionary<string, string> ttlMetadata, CancellationToken ct)
    {
        try
        {
            await _daprClient.SaveStateAsync(
                AppIds.StateStore,
                key,
                data,
                metadata: ttlMetadata,
                cancellationToken: ct);

            _logger.LogInformation("Cached {DataType}. Key={Key}, TTL={TTL}s",
                dataType, key, ttlMetadata["ttlInSeconds"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache {DataType}. Key={Key}", dataType, key);
        }
    }
    #endregion
}
