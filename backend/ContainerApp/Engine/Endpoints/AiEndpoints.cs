using Engine.Models;
using Engine.Models.Speech;
using Engine.Services;
using Microsoft.AspNetCore.Mvc;

namespace Engine.Endpoints;

public static class AiEndpoints
{
    private sealed class ManagerToAiEndpoint { }
    private sealed class ChatEndpoint { }

    public static WebApplication MapAiEndpoints(this WebApplication app)
    {
        #region HTTP POST

        app.MapPost("/chat", ChatAsync).WithName("ChatSync");

        app.MapPost("/speech/synthesize", SynthesizeAsync).WithName("SynthesizeText");

        #endregion

        return app;
    }

    private static async Task<IResult> ChatAsync(
      [FromBody] ChatRequestDto dto,
      [FromServices] IChatAiService ai,
      [FromServices] ILogger<ChatEndpoint> log,
      CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.UserMessage))
        {
            return Results.BadRequest(new { error = "userMessage is required" });
        }

        var aiReq = new AiRequestModel
        {
            Id = Guid.NewGuid().ToString("N"),
            ThreadId = string.IsNullOrWhiteSpace(dto.ThreadId)
                            ? Guid.NewGuid().ToString("N")
                            : dto.ThreadId,
            Question = dto.UserMessage,
            ReplyToQueue = string.Empty
        };

        var aiResp = await ai.ProcessAsync(aiReq, ct);

        if (aiResp.Status == "error")
        {
            return Results.Problem(aiResp.Error);
        }

        var result = new ChatResponseDto(aiResp.Answer ?? "", aiResp.ThreadId);
        log.LogInformation("Answered thread {Thread}", result.ThreadId);
        return Results.Ok(result);
    }

    private static async Task<IResult> SynthesizeAsync(
    [FromBody] SpeechRequestDto dto,
    [FromServices] ISpeechSynthesisService speechService,
    [FromServices] ILogger<ChatEndpoint> logger,
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during speech synthesis");
            return Results.Problem("An error occurred during speech synthesis.");
        }
    }
}