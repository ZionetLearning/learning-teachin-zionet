namespace IntegrationTests.Constants;

public static class ApiRoutes
{
    public const string Task = "/task";
    public static string TaskById(int id) => $"/task/{id}";
    public static string UpdateTaskName(int id, string name) => $"/task/{id}/{name}";

    public const string ChatMessage = "/threads/message";
    public static string ChatHistoryByThread(Guid threadId) => $"/threads/{threadId}/messages";
    public static string ChatThreadsByUser(string userId) => $"/threads/{userId}";

}
