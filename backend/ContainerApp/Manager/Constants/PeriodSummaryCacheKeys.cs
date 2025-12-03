namespace Manager.Constants;

public static class PeriodSummaryCacheKeys
{

    private const string HistoryPattern = "period-accessor:history:{0}:{1}:{2}"; // userId:startDate:endDate
    private const string WordCardsPattern = "period-accessor:word-cards:{0}:{1}:{2}";
    private const string AchievementsMapPattern = "period-accessor:achievements-map:{0}:{1}:{2}";
    private const string MistakesPattern = "period-accessor:mistakes:{0}:{1}:{2}";
    private const string AllAchievementsPattern = "period-accessor:all-achievements";

    private const string OverviewPattern = "period-summary:overview:{0}:{1}:{2}";
    private const string GamePracticePattern = "period-summary:game-practice:{0}:{1}:{2}";
    private const string WordCardsSummaryPattern = "period-summary:word-cards:{0}:{1}:{2}";
    private const string AchievementsSummaryPattern = "period-summary:achievements:{0}:{1}:{2}";

    // Level 1: Raw Accessor cache keys
    public static string History(Guid userId, DateTime startDate, DateTime endDate) =>
        string.Format(HistoryPattern, userId, startDate.ToString("yyyyMMdd"), endDate.ToString("yyyyMMdd"));

    public static string WordCards(Guid userId, DateTime startDate, DateTime endDate) =>
        string.Format(WordCardsPattern, userId, startDate.ToString("yyyyMMdd"), endDate.ToString("yyyyMMdd"));

    public static string AchievementsMap(Guid userId, DateTime startDate, DateTime endDate) =>
        string.Format(AchievementsMapPattern, userId, startDate.ToString("yyyyMMdd"), endDate.ToString("yyyyMMdd"));

    public static string Mistakes(Guid userId, DateTime startDate, DateTime endDate) =>
        string.Format(MistakesPattern, userId, startDate.ToString("yyyyMMdd"), endDate.ToString("yyyyMMdd"));

    public static string AllAchievements() => AllAchievementsPattern;

    // Level 2: Summary cache keys
    public static string Overview(Guid userId, DateTime startDate, DateTime endDate) =>
        string.Format(OverviewPattern, userId, startDate.ToString("yyyyMMdd"), endDate.ToString("yyyyMMdd"));

    public static string GamePractice(Guid userId, DateTime startDate, DateTime endDate) =>
        string.Format(GamePracticePattern, userId, startDate.ToString("yyyyMMdd"), endDate.ToString("yyyyMMdd"));

    public static string WordCardsSummary(Guid userId, DateTime startDate, DateTime endDate) =>
        string.Format(WordCardsSummaryPattern, userId, startDate.ToString("yyyyMMdd"), endDate.ToString("yyyyMMdd"));

    public static string AchievementsSummary(Guid userId, DateTime startDate, DateTime endDate) =>
        string.Format(AchievementsSummaryPattern, userId, startDate.ToString("yyyyMMdd"), endDate.ToString("yyyyMMdd"));

    // TTL for cache entries
    public const int DefaultTtlSeconds = 600; // 10 minutes
}
