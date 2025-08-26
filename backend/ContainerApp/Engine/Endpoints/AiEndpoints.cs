using Engine.Models.Chat;
using Engine.Models.Speech;
using Engine.Services;
using Engine.Services.Clients.AccessorClient;
using Engine.Services.Clients.AccessorClient.Models;
using Microsoft.AspNetCore.Mvc;

namespace Engine.Endpoints;

public static class AiEndpoints
{
    private sealed class ManagerToAiEndpoint { }
    private sealed class ChatEndpoint { }
    private sealed class SpeechEndpoint { }

    public static WebApplication MapAiEndpoints(this WebApplication app)
    {
        #region HTTP POST

        app.MapPost("/chat", ChatProcessAsync).WithName("ChatSync");

        app.MapPost("/speech/synthesize", SynthesizeAsync).WithName("SynthesizeText");

        #endregion

        return app;
    }

    private static async Task<IResult> ChatProcessAsync(
    [FromBody] EngineChatRequest request,
    [FromServices] IChatAiService ai,
    [FromServices] IAccessorClient accessorClient,
    [FromServices] ILogger<ChatEndpoint> log,
    CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.UserMessage))
        {
            return Results.BadRequest(new { error = "userMessage is required" });
        }

        var snapshot = await accessorClient.GetHistorySnapshotAsync(request.ThreadId, request.UserId, ct);

        var serviceRequest = new ChatAiServiseRequest
        {
            History = snapshot.History,
            UserMessage = request.UserMessage,
            ChatType = request.ChatType,
            ThreadId = request.ThreadId,
            UserId = request.UserId,
            Name = snapshot.Name,
            RequestId = request.RequestId,
            SentAt = request.SentAt,
            TtlSeconds = request.TtlSeconds,
        };

        var aiResponse = await ai.ChatHandlerAsync(serviceRequest, ct);

        if (aiResponse.Status != ChatAnswerStatus.Ok || aiResponse.Answer == null)
        {
            log.LogWarning("Answer for thread {Thread} failed. Error: {Error}", aiResponse.ThreadId, aiResponse.Error);
            return Results.Problem(aiResponse.Error ?? "AI failed.");
        }

        var upsert = new UpsertHistoryRequest
        {
            ThreadId = request.ThreadId,
            UserId = request.UserId,
            Name = aiResponse.Name,
            ChatType = request.ChatType.ToString().ToLowerInvariant(),
            History = aiResponse.UpdatedHistory
        };
        await accessorClient.UpsertHistorySnapshotAsync(upsert, ct);

        var responseToManager = new EngineChatResponse
        {
            AssistantMessage = aiResponse.Answer.Content,
            ChatName = aiResponse.Name,
            RequestId = request.RequestId,
            Status = aiResponse.Status,
            ThreadId = aiResponse.ThreadId
        };

        log.LogInformation("Answered thread {Thread}", responseToManager.ThreadId);
        return Results.Ok(responseToManager);
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