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

    public const string GamesAttempt = "games-manager/attempt";
    public static string GameHistory(Guid studentId) => $"games-manager/history/{studentId}";
    public static string GameMistakes(Guid studentId) => $"games-manager/mistakes/{studentId}";
    public const string GameAllHistory = "games-manager/all-history";

    // -------- Word Cards --------
    public const string WordCards = "wordcards-manager";
    public const string WordCardsUpdateLearnedStatus = "wordcards-manager/learned";

    // -------- User Game Config --------
    public const string GameConfig = "game-config-manager";
    public static string GameConfigByName(GameName gameName) => $"game-config-manager/{gameName}";



}
