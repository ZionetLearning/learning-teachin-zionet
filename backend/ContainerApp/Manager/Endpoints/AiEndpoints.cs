using Manager.Constants;
using Manager.Services;
using Manager.Models;

namespace Manager.Endpoints;

public static class AiEndpoints
{
    public static void MapAiEndpoints(this WebApplication app)
    {
        app.MapPost("/ai/question",
            async (AiRequestModel dto, IAiGatewayService aiService, ILoggerFactory logger, CancellationToken ct) =>
            {
                var log = logger.CreateLogger("AiEndpoints.Http");
                try
                {
                    var id = await aiService.SendQuestionAsync(dto.Question, ct);
                    log.LogInformation("Request {Id} accept", id);
                    return Results.Accepted($"/ai/answer/{id}", new { questionId = id });
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error sending question");
                    return Results.Problem("AI question failed");
                }
            });

        app.MapGet("/ai/answer/{id}",
            async (string id, IAiGatewayService ai, ILoggerFactory lf, CancellationToken ct) =>
            {
                var log = lf.CreateLogger("AiEndpoints.Http");
                var ans = await ai.GetAnswerAsync(id, ct);
                if (ans is null)
                {
                    log.LogDebug("The answer for {Id} is not ready yet", id);
                    return Results.NotFound(new { error = "Answer not ready" });
                }
                return Results.Ok(new { id, answer = ans });
            });

        app.MapPost($"/ai/{TopicNames.AiToManager}",
            async (AiResponseModel msg, IAiGatewayService ai, ILoggerFactory lf, CancellationToken ct) =>
            {
                var log = lf.CreateLogger("AiEndpoints.PubSub");
                try
                {
                    await ai.SaveAnswerAsync(msg, ct);
                    log.LogInformation("Answer saved");
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error saving answer");
                    return Results.Problem("AI answer handling failed");
                }
            })
            .WithTopic("pubsub", TopicNames.AiToManager);
        
    }
}