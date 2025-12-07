using IntegrationTests.Constants;
using Manager.Models.Chat;
using Manager.Services.Clients.Engine.Models;
using System.Net.Http.Json;

public static class AIChatHelper
{
    // Возвращаем теперь ОБЪЕКТ, а не список
    public static async Task<GetChatHistoryResponse> CheckCountMessageInChatHistory(
        HttpClient client,
        Guid chatId,
        Guid userId,
        int waitMessages,
        int timeoutSeconds = 60)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync(AiRoutes.GetHistory(chatId, userId));

            if (response.IsSuccessStatusCode)
            {
                // Десериализуем обертку
                var historyWrapper = await response.Content.ReadFromJsonAsync<GetChatHistoryResponse>();


                // Проверяем count внутри свойства Messages
                if (historyWrapper?.Messages != null && historyWrapper.Messages.Count >= waitMessages)
                {
                    return historyWrapper;
                }
            }

            await Task.Delay(1000);
        }

        throw new TimeoutException($"Timed out waiting new messages for chat {chatId}.");
    }
}