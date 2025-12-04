using System.Text.Json;
using Engine.Models.Chat;
using Engine.Services;
using Engine.Services.Clients.AccessorClient;
using Microsoft.AspNetCore.Mvc;

namespace Engine.Endpoints;

public static class AiEndpoints
{
    private sealed class ManagerToAiEndpoint { }
    private sealed class ChatEndpoint { }
    private sealed class SpeechEndpoint { }

    public static WebApplication MapAiEndpoints(this WebApplication app)
    {
        #region HTTP GET

        app.MapGet("/chat/{chatId:guid}/{userId:guid}/history", GetHistoryChatAsync).WithName("GetHistoryChat");

        #endregion

        #region HTTP POST
        #endregion

        return app;
    }

    private static async Task<IResult> GetHistoryChatAsync(
        Guid chatId,
        Guid userId,
        [FromServices] IChatAiService ai,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<ChatEndpoint> log,
        CancellationToken ct)
    {
        log.LogInformation("Start method: {Method}, chatId {ChatId}, userId:{UserId}", nameof(GetHistoryChatAsync), chatId, userId);

        var snapshot = await accessorClient.GetHistorySnapshotAsync(chatId, userId, ct);

        if (snapshot is null)
        {
            return Results.Ok(new ChatHistoryForFrontDto
            {
                ChatId = chatId,
                Messages = new List<OpenAiMessageDto>()
            });
        }

        var messages = new List<OpenAiMessageDto>();

        if (snapshot.History.ValueKind != JsonValueKind.Undefined && snapshot.History.ValueKind != JsonValueKind.Null)
        {
            var formattedMessages = ai.GetFormattedHistory(snapshot.History);
            if (formattedMessages is not null)
            {
                messages = formattedMessages.ToList();
            }
        }

        var payload = new ChatHistoryForFrontDto
        {
            ChatId = snapshot.ThreadId,
            Name = snapshot.Name,
            ChatType = snapshot.ChatType,
            Messages = messages?.ToList()
        };

        return Results.Ok(payload);
    }
}