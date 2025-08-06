using Engine.Models;
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
}