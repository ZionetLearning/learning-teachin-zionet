using IntegrationTests.Constants;
using IntegrationTests.Models.Ai.Chat;
using System.Net.Http.Json;

public static class AIChatHelper
{

    public static async Task<ChatHistoryForFrontDto> CheckCountMessageInChatHistory(HttpClient client, Guid chatId, Guid userId, int waitMessages, int timeoutSeconds = 60)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync(AiRoutes.GetHistory(chatId, userId));
            if (response.IsSuccessStatusCode)
            {
                var chat = await response.Content.ReadFromJsonAsync<ChatHistoryForFrontDto>();
                if (chat?.Messages?.Count >= waitMessages)
                    return chat;
            }

            await Task.Delay(1000);
        }

        throw new TimeoutException($"Timed out waiting new messages for chat {chatId}.");
    }
}