﻿using Engine.Helpers;
using Engine.Models.Speech;
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

        app.MapPost("/speech/synthesize", SynthesizeAsync).WithName("SynthesizeText");

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

        var payload = HistoryMapper.MapHistoryForFront(snapshot);

        return Results.Ok(payload);
    }

    private static async Task<IResult> SynthesizeAsync(
    [FromBody] SpeechRequestDto dto,
    [FromServices] ISpeechSynthesisService speechService,
    [FromServices] ILogger<SpeechEndpoint> logger,
    CancellationToken ct)
    {
        logger.LogInformation("Processing speech synthesis request for text length: {Length}", dto.Text.Length);

        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            return Results.BadRequest(new { error = "Text is required" });
        }

        if (dto.Text.Length > 1000)
        {
            return Results.BadRequest(new { error = "Text length cannot exceed 1000 characters" });
        }

        try
        {
            var result = await speechService.SynthesizeAsync(dto, ct);

            if (result != null)
            {
                logger.LogInformation("Speech synthesis completed successfully");
                return Results.Ok(result);
            }
            else
            {
                logger.LogError("Speech synthesis failed - service returned null");
                return Results.Problem("Speech synthesis failed.");
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Speech synthesis operation was canceled by user");
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Speech synthesis operation timed out");
            return Results.StatusCode(StatusCodes.Status408RequestTimeout);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during speech synthesis");
            return Results.Problem("An error occurred during speech synthesis.");
        }
    }
}