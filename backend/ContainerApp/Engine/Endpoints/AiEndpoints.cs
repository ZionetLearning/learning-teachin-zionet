using Engine.Constants;
using Engine.Models;
using Engine.Services;
using Microsoft.AspNetCore.Mvc;

namespace Engine.Endpoints;

public static class AiEndpoints
{
    private sealed class ManagerToAiEndpoint { }

    public static WebApplication MapAiEndpoints(this WebApplication app)
    {
        #region HTTP POST

        //app.MapPost($"/{TopicNames.ManagerToAi}", ProcessQuestionAsync).WithTopic("pubsub", TopicNames.ManagerToAi);

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
            if (string.IsNullOrWhiteSpace(req.ThreadId)) return Results.BadRequest("ThreadId is required.");

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


}