namespace IntegrationTests.Constants;

public class AiRoutes
{
    public const string PostNewMessage = "ai-manager/chat";
    public static string GetHistory(Guid chatId, Guid userId) => $"ai-manager/chat/{chatId}/{userId}";

    public const string GetChats = "ai-manager/chats";
}
