using System.Text.Json;
using Accessor.Models;
using Accessor.Services;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class ChatsEndpoints
{
    public static IEndpointRouteBuilder MapChatsEndpoints(this IEndpointRouteBuilder app)
    {
        var chatsGroup = app.MapGroup("/chats-accessor").WithTags("Chats");

        // POST /chats-accessor/history
        chatsGroup.MapPost("/history", UpsertHistorySnapshotAsync).WithName("UpsertHistorySnapshot");

        // GET /chats-accessor/{threadId}/{userId}/history
        chatsGroup.MapGet("/{threadId:guid}/{userId:guid}/history", GetHistorySnapshotAsync).WithName("GetHistorySnapshot");

        // GET /chats-accessor/{userId}
        chatsGroup.MapGet("/{userId:guid}", GetChatsForUserAsync).WithName("GetChatsForUser");

        return app;
    }

    // Accessor/Endpoints/ChatsEndpoints.cs (фрагмент UpsertHistorySnapshotAsync)
    private static async Task<IResult> UpsertHistorySnapshotAsync(
        [FromBody] UpsertHistoryRequest body,
        [FromServices] IChatService chatService,
        [FromServices] ILogger<ChatService> logger)
    {
        using var _ = logger.BeginScope("Handler: {Handler}, ThreadId: {ThreadId}",
            nameof(UpsertHistorySnapshotAsync), body.ThreadId);

        try
        {
            if (body.ThreadId == Guid.Empty)
            {
                return Results.BadRequest(new { error = "threadId is required and must be a GUID." });
            }

            if (body.UserId == Guid.Empty)
            {
                return Results.BadRequest(new { error = "UserId is required and must be a GUID." });
            }

            var rawHistory =
                body.History.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null
                    ? "null"
                    : body.History.GetRawText();

            var existing = await chatService.GetHistorySnapshotAsync(body.ThreadId);

            var snapshot = new ChatHistorySnapshot
            {
                ThreadId = body.ThreadId,
                UserId = body.UserId,
                ChatType = string.IsNullOrWhiteSpace(body.ChatType) ? "default" : body.ChatType!,
                Name = string.IsNullOrWhiteSpace(body.Name) ? (existing?.Name ?? "New chat") : body.Name!,
                History = rawHistory,
                CreatedAt = existing?.CreatedAt ?? DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await chatService.UpsertHistorySnapshotAsync(snapshot);

            JsonElement historyForResponse;
            using (var doc = JsonDocument.Parse(rawHistory))
            {
                historyForResponse = doc.RootElement.Clone();
            }

            var payload = new
            {
                threadId = snapshot.ThreadId,
                snapshot.UserId,
                snapshot.ChatType,
                snapshot.Name,
                history = historyForResponse
            };

            return existing is null
                ? Results.CreatedAtRoute("GetHistorySnapshot", new { threadId = snapshot.ThreadId, userId = snapshot.UserId }, payload)
                : Results.Ok(payload);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting history snapshot for thread {ThreadId}", body.ThreadId);
            return Results.Problem("An error occurred while storing the chat history snapshot.");
        }
    }

    private static async Task<IResult> GetHistorySnapshotAsync(
    Guid threadId,
    Guid userId,
    [FromServices] IChatService chatService,
    [FromServices] ILogger<ChatService> logger)
    {
        using var _ = logger.BeginScope("Handler: {Handler}, ThreadId: {ThreadId}", nameof(GetHistorySnapshotAsync), threadId);

        try
        {
            var snapshot = await chatService.GetHistorySnapshotAsync(threadId);
            if (snapshot is null)
            {
                snapshot = new ChatHistorySnapshot
                {
                    ThreadId = threadId,
                    UserId = userId,
                    Name = "New chat",
                    ChatType = "default",
                    History = "null",
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                await chatService.CreateChatAsync(snapshot);
                logger.LogInformation("Created new chat {ChatId}", threadId);
            }
            else if (snapshot.UserId != userId)
            {
                logger.LogWarning("User {UserId} requested thread {ThreadId} owned by {OwnerId}",
                    userId, threadId, snapshot.UserId);
                return Results.Forbid();

            }

            JsonElement historySafe;
            using (var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(snapshot.History) ? "null" : snapshot.History))
            {
                historySafe = doc.RootElement.Clone();
            }

            return Results.Ok(new
            {
                threadId = snapshot.ThreadId,
                snapshot.UserId,
                snapshot.ChatType,
                snapshot.Name,
                history = historySafe
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching history snapshot for thread {ThreadId}", threadId);
            return Results.Problem("An error occurred while retrieving chat history snapshot.");
        }
    }

    private static async Task<IResult> GetChatsForUserAsync(
        Guid userId,
        [FromServices] IChatService chatService,
        [FromServices] ILogger<ChatService> logger)
    {
        using var scope = logger.BeginScope("Handler: {Handler}, UserId: {UserId}", nameof(GetChatsForUserAsync), userId);
        try
        {
            var chats = await chatService.GetChatsForUserAsync(userId);
            logger.LogInformation("Retrieved chats for user");
            return Results.Ok(chats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing chats for user {UserId}", userId);
            return Results.Problem("An error occurred while listing chats.");
        }
    }
}
