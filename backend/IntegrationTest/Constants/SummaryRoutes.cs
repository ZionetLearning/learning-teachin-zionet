namespace IntegrationTests.Constants;

public static class SummaryRoutes
{
    private const string BaseRoute = "period-summary";

    public static string GetPeriodOverview(Guid userId) => $"{BaseRoute}/{userId}/overview";
    public static string GetPeriodGamePractice(Guid userId) => $"{BaseRoute}/{userId}/game-practice";
    public static string GetPeriodWordCards(Guid userId) => $"{BaseRoute}/{userId}/word-cards";
    public static string GetPeriodAchievements(Guid userId) => $"{BaseRoute}/{userId}/achievements";

    public static string GetPeriodOverviewWithDates(Guid userId, DateTime startDate, DateTime endDate) =>
        $"{BaseRoute}/{userId}/overview?startDate={startDate:O}&endDate={endDate:O}";

    public static string GetPeriodGamePracticeWithDates(Guid userId, DateTime startDate, DateTime endDate) =>
        $"{BaseRoute}/{userId}/game-practice?startDate={startDate:O}&endDate={endDate:O}";

    public static string GetPeriodWordCardsWithDates(Guid userId, DateTime startDate, DateTime endDate) =>
        $"{BaseRoute}/{userId}/word-cards?startDate={startDate:O}&endDate={endDate:O}";

    public static string GetPeriodAchievementsWithDates(Guid userId, DateTime startDate, DateTime endDate) =>
        $"{BaseRoute}/{userId}/achievements?startDate={startDate:O}&endDate={endDate:O}";
}
