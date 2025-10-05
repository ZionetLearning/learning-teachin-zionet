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



    //---------Sentences--------
    public const string Sentences = "ai-manager/sentence";
    public const string SplitSentences = "ai-manager/sentence/split";

    // -------- Games --------
    public const string GameAttempt = "games-manager/attempt";
    public static string GameHistory(Guid studentId) => $"games-manager/history/{studentId}";
    public static string GameMistakes(Guid studentId) => $"games-manager/mistakes/{studentId}";
    public const string GameAllHistory = "games-manager/all-history";
}
