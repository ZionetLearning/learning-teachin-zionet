using Engine.Models;

namespace Engine.Services.Clients;

public interface IAccessorClient
{
    Task<ChatHistoryResponse?> GetChatHistoryAsync(string threadId);
    Task<bool> StoreChatMessagesAsync(StoreChatMessagesRequest request);
}