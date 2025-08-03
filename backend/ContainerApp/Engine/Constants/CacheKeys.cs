namespace Engine.Constants;

public static class CacheKeys
{
    private const string ChatHistoryPattern = "chat-history:{0}";

    public static string ChatHistory(string threadId) => string.Format(ChatHistoryPattern, threadId);
}