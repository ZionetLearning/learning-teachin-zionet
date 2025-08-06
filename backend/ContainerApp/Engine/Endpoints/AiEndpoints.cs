using Engine.Constants;
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

        app.MapPost($"/{TopicNames.ManagerToAi}", ProcessQuestionAsync).WithTopic("pubsub", TopicNames.ManagerToAi);
        app.MapPost("/chat", ChatAsync).WithName("ChatSync");

        #endregion

        return app;
    }

    private static async Task<IResult> ProcessQuestionAsync(
        [FromBody] AiRequestModel req,
        [FromServices] IChatAiService aiService,
        [FromServices] IAiReplyPublisher publisher,
        [FromServices] ILogger<ManagerToAiEndpoint> log,
        CancellationToken ct)
    {
        log.LogInformation("Received AI question {Id} from manager", req.Id);
        try
        {
            if (string.IsNullOrWhiteSpace(req.ThreadId))
            {
                return Results.BadRequest("ThreadId is required.");
            }

            var response = await aiService.ProcessAsync(req, ct);

            await publisher.PublishAsync(response, req.ReplyToTopic, ct);

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error processing AI question {Id}", req.Id);
            return Results.Problem("An error occurred while processing the AI question.");
        }
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
            ReplyToTopic = string.Empty
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