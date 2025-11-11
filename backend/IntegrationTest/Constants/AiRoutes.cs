namespace IntegrationTests.Constants;

public class AiRoutes
{
    public const string PostNewMessageChat = "ai-manager/chat";
    public const string PostNewMessageGlobalChat = "ai-manager/global-chat";

    public static string GetHistory(Guid chatId, Guid userId) => $"ai-manager/chat/{chatId}/{userId}";

    public const string GetChats = "ai-manager/chats";
}
