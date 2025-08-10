namespace IntegrationTests.Constants;

public static class ApiRoutes
{
    public const string Task = "/task";
    public static string TaskById(int id) => $"/task/{id}";
    public static string UpdateTaskName(int id, string name) => $"/task/{id}/{name}";

    public const string ChatMessage = "/chat-history/message";
    public static string ChatHistoryByThread(Guid threadId) => $"/chat-history/{threadId}";
    public static string ChatMessagesByUser(string userId) => $"/chat-history/threads/{userId}";


}
