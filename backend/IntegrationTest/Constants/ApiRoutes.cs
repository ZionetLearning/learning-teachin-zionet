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

    //---------Games--------

    public const string GamesAttempt = "games-manager/attempt";

    //---------Chat--------

    public const string ChatMistakeExplanation = "ai-manager/chat/mistake-explanation";
}
