using Manager.Models;
using Manager.Models.Games;
using Manager.Models.Summaries;
using Manager.Services.Clients.Accessor.Interfaces;
using Manager.Services.Clients.Accessor.Models.Games;
using Manager.Services.Clients.Accessor.Models.WordCards;
using Manager.Services.Clients.Accessor.Models.Achievements;

namespace Manager.Services.PeriodSummary;

public class PeriodSummarizerService : IPeriodSummarizerService
{
    private readonly IGameAccessorClient _gameAccessorClient;
    private readonly IWordCardsAccessorClient _wordCardsAccessorClient;
    private readonly IAchievementAccessorClient _achievementAccessorClient;
    private readonly IPeriodSummaryCacheService _cacheService;
    private readonly ILogger<PeriodSummarizerService> _logger;

    public PeriodSummarizerService(
        IGameAccessorClient gameAccessorClient,
        IWordCardsAccessorClient wordCardsAccessorClient,
        IAchievementAccessorClient achievementAccessorClient,
        IPeriodSummaryCacheService cacheService,
        ILogger<PeriodSummarizerService> logger)
    {
        _gameAccessorClient = gameAccessorClient;
        _wordCardsAccessorClient = wordCardsAccessorClient;
        _achievementAccessorClient = achievementAccessorClient;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<GetPeriodOverviewResponse> GetPeriodOverviewAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var cachedSummary = await _cacheService.GetCachedOverviewAsync(userId, startDate, endDate, ct);
        if (cachedSummary is not null)
        {
            _logger.LogDebug("Returning cached overview summary for UserId={UserId}", userId);
            return cachedSummary;
        }

        try
        {
            _logger.LogInformation("Building period overview for UserId={UserId}, StartDate={StartDate}, EndDate={EndDate}",
                userId, startDate, endDate);

            var fromDate = new DateTimeOffset(startDate, TimeSpan.Zero);
            var toDate = new DateTimeOffset(endDate, TimeSpan.Zero);

            var historyTask = GetOrFetchHistoryAsync(userId, fromDate, toDate, ct);
            var wordCardsTask = GetOrFetchWordCardsAsync(userId, startDate, endDate, ct);
            var achievementsTask = GetOrFetchAchievementsMapAsync(userId, startDate, endDate, ct);

            await Task.WhenAll(historyTask, wordCardsTask, achievementsTask);

            var history = await historyTask;
            var wordCards = await wordCardsTask;
            var achievements = await achievementsTask;

            var totalAttempts = history.Detailed?.TotalCount ?? 0;
            var wordsLearned = wordCards.WordCards.Count(c => c.IsLearned);
            var achievementsUnlocked = achievements.Count;

            var practiceDays = history.Detailed?.Items
                .Select(s => s.CreatedAt.Date)
                .Distinct()
                .Count() ?? 0;

            var response = new GetPeriodOverviewResponse
            {
                Period = new DateRangePeriod
                {
                    StartDate = startDate,
                    EndDate = endDate
                },
                Summary = new PeriodOverviewSummary
                {
                    TotalAttempts = totalAttempts,
                    WordsLearned = wordsLearned,
                    AchievementsUnlocked = achievementsUnlocked,
                    PracticeDays = practiceDays
                }
            };

            await _cacheService.CacheOverviewAsync(userId, startDate, endDate, response, ct);

            _logger.LogInformation("Period overview built successfully for UserId={UserId}", userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build period overview for UserId={UserId}", userId);
            throw;
        }
    }

    public async Task<GetGamePracticeSummaryResponse> GetPeriodGamePracticeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var cachedSummary = await _cacheService.GetCachedGamePracticeAsync(userId, startDate, endDate, ct);
        if (cachedSummary is not null)
        {
            _logger.LogDebug("Returning cached game practice summary for UserId={UserId}", userId);
            return cachedSummary;
        }

        try
        {
            _logger.LogInformation("Building period game practice (with mistakes) for UserId={UserId}, StartDate={StartDate}, EndDate={EndDate}",
                userId, startDate, endDate);

            var fromDate = new DateTimeOffset(startDate, TimeSpan.Zero);
            var toDate = new DateTimeOffset(endDate, TimeSpan.Zero);

            // Fetch both history and mistakes data in parallel (Level 1 cache)
            var historyTask = GetOrFetchHistoryAsync(userId, fromDate, toDate, ct);
            var mistakesTask = GetOrFetchMistakesAsync(userId, fromDate, toDate, ct);

            await Task.WhenAll(historyTask, mistakesTask);

            var history = await historyTask;
            var mistakes = await mistakesTask;

            var attempts = history.Detailed?.Items.ToList() ?? [];
            var mistakesList = mistakes.Items.ToList();

            // Calculate overall summary
            var totalAttempts = attempts.Count;
            var successfulAttempts = attempts.Count(a => a.Status == AttemptStatus.Success);
            var successRate = totalAttempts > 0 ? (decimal)successfulAttempts / totalAttempts : 0;
            var averageAccuracy = attempts.Count > 0 ? attempts.Average(a => a.Accuracy) : 0;

            // Group by game type with mistakes count
            var mistakesByGameType = mistakesList
                .GroupBy(m => m.GameType.ToString())
                .ToDictionary(g => g.Key, g => g.Sum(m => m.Mistakes.Count));

            var byGameType = attempts
                .GroupBy(a => a.GameType)
                .Select(g =>
                {
                    var gameType = g.Key.ToString();
                    mistakesByGameType.TryGetValue(gameType, out var mistakesCount);
                    return new GameTypeStats
                    {
                        GameType = gameType,
                        Attempts = g.Count(),
                        SuccessRate = g.Any() ? (decimal)g.Count(a => a.Status == AttemptStatus.Success) / g.Count() : 0,
                        AverageAccuracy = g.Any() ? g.Average(a => a.Accuracy) : 0,
                        MistakesCount = mistakesCount
                    };
                })
                .ToList();

            var daily = attempts
                .GroupBy(a => a.CreatedAt.Date)
                .Select(g => new DailyGameStats
                {
                    Date = g.Key,
                    Attempts = g.Count(),
                    SuccessRate = g.Any() ? (decimal)g.Count(a => a.Status == AttemptStatus.Success) / g.Count() : 0
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Mistakes data (only uncorrected exercises)
            var totalUncorrectedMistakes = mistakesList.Sum(m => m.Mistakes.Count);
            var retriedButNotCorrected = mistakesList.Count(m => m.Mistakes.Count > 1);
            var uncorrectedExercises = mistakesList.Count;

            var patterns = mistakesList
                .GroupBy(m => new { m.GameType, m.Difficulty })
                .Select(g => new MistakePattern
                {
                    GameType = g.Key.GameType.ToString(),
                    Difficulty = g.Key.Difficulty.ToString(),
                    Count = g.Sum(m => m.Mistakes.Count)
                })
                .OrderByDescending(p => p.Count)
                .Take(10)
                .ToList();

            var uncorrectedExamples = mistakesList
                .SelectMany(exercise => exercise.Mistakes.Select(mistake => new
                {
                    Exercise = exercise,
                    Mistake = mistake
                }))
                .OrderByDescending(m => m.Mistake.CreatedAt)
                .Take(10)
                .Select((m, index) => new MistakeExample
                {
                    ExerciseId = m.Exercise.ExerciseId,
                    GameType = m.Exercise.GameType.ToString(),
                    Difficulty = m.Exercise.Difficulty.ToString(),
                    CorrectAnswer = m.Exercise.CorrectAnswer.ToList(),
                    GivenAnswer = m.Mistake.WrongAnswer.ToList(),
                    Accuracy = m.Mistake.Accuracy,
                    AttemptNumber = index + 1,
                    CreatedAt = m.Mistake.CreatedAt.UtcDateTime
                })
                .ToList();

            var response = new GetGamePracticeSummaryResponse
            {
                Summary = new GamePracticeSummary
                {
                    TotalAttempts = totalAttempts,
                    SuccessRate = successRate,
                    AverageAccuracy = averageAccuracy
                },
                ByGameType = byGameType,
                Daily = daily,
                Mistakes = new MistakesData
                {
                    Summary = new MistakesSummary
                    {
                        TotalUncorrectedMistakes = totalUncorrectedMistakes,
                        RetriedButNotCorrected = retriedButNotCorrected,
                        UncorrectedExercises = uncorrectedExercises
                    },
                    Patterns = patterns,
                    UncorrectedExamples = uncorrectedExamples
                }
            };

            await _cacheService.CacheGamePracticeAsync(userId, startDate, endDate, response, ct);

            _logger.LogInformation("Period game practice (with mistakes) built successfully for UserId={UserId}", userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build period game practice for UserId={UserId}", userId);
            throw;
        }
    }

    public async Task<GetPeriodWordCardsResponse> GetPeriodWordCardsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var cachedSummary = await _cacheService.GetCachedWordCardsSummaryAsync(userId, startDate, endDate, ct);
        if (cachedSummary is not null)
        {
            _logger.LogDebug("Returning cached word cards summary for UserId={UserId}", userId);
            return cachedSummary;
        }

        try
        {
            _logger.LogInformation("Building period word cards for UserId={UserId}, StartDate={StartDate}, EndDate={EndDate}",
                userId, startDate, endDate);

            var periodCards = await GetOrFetchWordCardsAsync(userId, startDate, endDate, ct);

            var newInPeriod = periodCards.WordCards.Count;
            var learnedInPeriod = periodCards.WordCards.Count(c => c.IsLearned);

            var summary = new WordCardsSummary
            {
                NewInPeriod = newInPeriod,
                LearnedInPeriod = learnedInPeriod,
            };

            var recentLearned = periodCards.WordCards
                .Where(c => c.IsLearned)
                .Take(10)
                .Select(c => new WordCardInfo
                {
                    CardId = c.CardId,
                    Hebrew = c.Hebrew,
                    English = c.English,
                    Timestamp = c.UpdatedAt
                })
                .ToList();

            var newCards = periodCards.WordCards
                .Take(10)
                .Select(c => new WordCardInfo
                {
                    CardId = c.CardId,
                    Hebrew = c.Hebrew,
                    English = c.English,
                    Timestamp = c.CreatedAt
                })
                .ToList();

            var response = new GetPeriodWordCardsResponse
            {
                Summary = summary,
                RecentLearned = recentLearned,
                NewCards = newCards
            };

            await _cacheService.CacheWordCardsSummaryAsync(userId, startDate, endDate, response, ct);

            _logger.LogInformation("Period word cards built successfully for UserId={UserId}", userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build period word cards for UserId={UserId}", userId);
            throw;
        }
    }

    public async Task<GetPeriodAchievementsResponse> GetPeriodAchievementsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var cachedSummary = await _cacheService.GetCachedAchievementsSummaryAsync(userId, startDate, endDate, ct);
        if (cachedSummary is not null)
        {
            _logger.LogDebug("Returning cached achievements summary for UserId={UserId}", userId);
            return cachedSummary;
        }

        try
        {
            _logger.LogInformation("Building period achievements for UserId={UserId}, StartDate={StartDate}, EndDate={EndDate}",
                userId, startDate, endDate);

            var unlockedMap = await GetOrFetchAchievementsMapAsync(userId, startDate, endDate, ct);
            var allAchievements = await GetOrFetchAllAchievementsAsync(ct);

            var unlockedInPeriod = allAchievements
                .Where(a => unlockedMap.ContainsKey(a.AchievementId))
                .Select(a => new UnlockedAchievement
                {
                    AchievementId = a.AchievementId,
                    Name = a.Name ?? string.Empty,
                    Description = a.Description ?? string.Empty,
                    Feature = a.Feature ?? string.Empty,
                    UnlockedAt = unlockedMap[a.AchievementId]
                })
                .OrderByDescending(a => a.UnlockedAt)
                .ToList();

            var response = new GetPeriodAchievementsResponse
            {
                UnlockedInPeriod = unlockedInPeriod
            };

            await _cacheService.CacheAchievementsSummaryAsync(userId, startDate, endDate, response, ct);

            _logger.LogInformation("Period achievements built successfully for UserId={UserId}", userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build period achievements for UserId={UserId}", userId);
            throw;
        }
    }

    // ==================== Helper Methods for Level 1 Caching ====================

    private async Task<GetHistoryAccessorResponse> GetOrFetchHistoryAsync(Guid userId, DateTimeOffset fromDate, DateTimeOffset toDate, CancellationToken ct)
    {
        var startDate = fromDate.UtcDateTime;
        var endDate = toDate.UtcDateTime;

        var cached = await _cacheService.GetCachedHistoryAsync(userId, startDate, endDate, ct);
        if (cached is not null)
        {
            _logger.LogDebug("Using cached history from Accessor for UserId={UserId}", userId);
            return cached;
        }

        _logger.LogDebug("Fetching history from Accessor for UserId={UserId}", userId);

        var allItems = new List<AttemptHistoryDto>();
        var currentPage = 1;
        const int pageSize = 100;
        var totalCount = 0;

        while (true)
        {
            var pageResult = await _gameAccessorClient.GetHistoryAsync(
                userId,
                summary: false,
                page: currentPage,
                pageSize: pageSize,
                getPending: false,
                fromDate,
                toDate,
                ct);

            if (pageResult.Detailed?.Items != null)
            {
                allItems.AddRange(pageResult.Detailed.Items);
                totalCount = pageResult.Detailed.TotalCount;

                _logger.LogDebug(
                    "Fetched page {Page} of history for UserId={UserId}. Items={Count}, TotalCount={Total}",
                    currentPage, userId, pageResult.Detailed.Items.Count, totalCount);

                if (!pageResult.Detailed.HasNextPage)
                {
                    break;
                }

                currentPage++;
            }
            else
            {
                break;
            }
        }

        var history = new GetHistoryAccessorResponse
        {
            Detailed = new PagedResult<AttemptHistoryDto>
            {
                Items = allItems,
                Page = 1,
                PageSize = allItems.Count,
                TotalCount = totalCount
            }
        };

        _logger.LogInformation(
            "Fetched complete history for UserId={UserId}. TotalItems={Count}, TotalPages={Pages}",
            userId, allItems.Count, currentPage);

        await _cacheService.CacheHistoryAsync(userId, startDate, endDate, history, ct);

        return history;
    }

    private async Task<GetWordCardsAccessorResponse> GetOrFetchWordCardsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct)
    {
        var cached = await _cacheService.GetCachedWordCardsAsync(userId, startDate, endDate, ct);
        if (cached is not null)
        {
            _logger.LogDebug("Using cached word cards from Accessor for UserId={UserId}", userId);
            return cached;
        }

        _logger.LogDebug("Fetching word cards from Accessor for UserId={UserId}", userId);
        var wordCards = await _wordCardsAccessorClient.GetWordCardsAsync(userId, startDate, endDate, ct);

        await _cacheService.CacheWordCardsAsync(userId, startDate, endDate, wordCards, ct);

        return wordCards;
    }

    private async Task<IReadOnlyDictionary<Guid, DateTime>> GetOrFetchAchievementsMapAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct)
    {
        var cached = await _cacheService.GetCachedAchievementsMapAsync(userId, startDate, endDate, ct);
        if (cached is not null)
        {
            _logger.LogDebug("Using cached achievements map from Accessor for UserId={UserId}", userId);
            return cached;
        }

        _logger.LogDebug("Fetching achievements map from Accessor for UserId={UserId}", userId);
        var achievementsMap = await _achievementAccessorClient.GetUserUnlockedAchievementsAsync(userId, startDate, endDate, ct);

        await _cacheService.CacheAchievementsMapAsync(userId, startDate, endDate, achievementsMap, ct);

        return achievementsMap;
    }

    private async Task<GetMistakesAccessorResponse> GetOrFetchMistakesAsync(Guid userId, DateTimeOffset fromDate, DateTimeOffset toDate, CancellationToken ct)
    {
        var startDate = fromDate.UtcDateTime;
        var endDate = toDate.UtcDateTime;

        var cached = await _cacheService.GetCachedMistakesAsync(userId, startDate, endDate, ct);
        if (cached is not null)
        {
            _logger.LogDebug("Using cached mistakes from Accessor for UserId={UserId}", userId);
            return cached;
        }

        _logger.LogDebug("Fetching mistakes from Accessor for UserId={UserId}", userId);

        var allItems = new List<ExerciseMistakes>();
        var currentPage = 1;
        const int pageSize = 100;
        var totalCount = 0;

        while (true)
        {
            var pageResult = await _gameAccessorClient.GetMistakesAsync(
                userId,
                page: currentPage,
                pageSize: pageSize,
                fromDate,
                toDate,
                ct);

            if (pageResult.Items != null)
            {
                allItems.AddRange(pageResult.Items);
                totalCount = pageResult.TotalCount;

                _logger.LogDebug(
                    "Fetched page {Page} of mistakes for UserId={UserId}. Items={Count}, TotalCount={Total}",
                    currentPage, userId, pageResult.Items.Count, totalCount);

                if (!pageResult.HasNextPage)
                {
                    break;
                }

                currentPage++;
            }
            else
            {
                break;
            }
        }

        var mistakes = new GetMistakesAccessorResponse
        {
            Items = allItems,
            Page = 1,
            PageSize = allItems.Count,
            TotalCount = totalCount,
            HasNextPage = false
        };

        _logger.LogInformation(
            "Fetched complete mistakes for UserId={UserId}. TotalItems={Count}, TotalPages={Pages}",
            userId, allItems.Count, currentPage);

        await _cacheService.CacheMistakesAsync(userId, startDate, endDate, mistakes, ct);

        return mistakes;
    }

    private async Task<IReadOnlyList<AchievementAccessorModel>> GetOrFetchAllAchievementsAsync(CancellationToken ct)
    {
        var cached = await _cacheService.GetCachedAllAchievementsAsync(ct);
        if (cached is not null)
        {
            _logger.LogDebug("Using cached all achievements from Accessor");
            return cached;
        }

        _logger.LogDebug("Fetching all achievements from Accessor");
        var allAchievements = await _achievementAccessorClient.GetAllActiveAchievementsAsync(ct: ct);

        await _cacheService.CacheAllAchievementsAsync(allAchievements, ct);

        return allAchievements;
    }
}
