using Manager.Models.UserGameConfiguration;

namespace IntegrationTests.Constants;

public static class ApiRoutes
{
    // -------- Tasks --------
    public const string Task = "tasks-manager/task";
    public static string TaskById(int id) => $"tasks-manager/task/{id}";
    public static string UpdateTaskName(int id, string name) => $"tasks-manager/task/{id}/{name}";

    public const string TasksList = "tasks-manager/tasks";

    // -------- Users --------
    public const string User = "users-manager/user";
    public static string UserById(Guid userId) => $"users-manager/user/{userId}";
    public const string UserList = "users-manager/user-list";
    public static string UserSetInterests(Guid userId) => $"users-manager/user/interests/{userId}";

    // -------- Avatar --------
    public static string AvatarUploadUrl(Guid userId)
        => $"users-manager/user/{userId}/avatar/upload-url";

    public static string AvatarConfirm(Guid userId)
        => $"users-manager/user/{userId}/avatar/confirm";

    public static string AvatarReadUrl(Guid userId)
        => $"users-manager/user/{userId}/avatar/url";

    public static string AvatarDelete(Guid userId)
        => $"users-manager/user/{userId}/avatar";
    
    //---------Sentences--------
    public const string Sentences = "ai-manager/sentence";
    public const string SplitSentences = "ai-manager/sentence/split";

    //---------Chat--------

    public const string ChatMistakeExplanation = "ai-manager/chat/mistake-explanation";

    // -------- Games --------
    // NOTE: Game routes have been moved to GamesRoutes.cs

    // -------- Word Cards --------
    public const string WordCards = "wordcards-manager";
    public const string WordCardsUpdateLearnedStatus = "wordcards-manager/learned";

    // -------- User Game Config --------
    public const string GameConfig = "game-config-manager";
    public static string GameConfigByName(GameName gameName) => $"game-config-manager/{gameName}";

    // -------- Summaries --------

    private const string BaseRoute = "/summaries-manager/summary";

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

    // -------- Emails --------
    public const string EmailDraft = "emails-manager/draft";
    public static string GetEmailRecipients(string name) => $"emails-manager/recipients/{name}";

    public const string SendEmail = "emails-manager/send";


}
