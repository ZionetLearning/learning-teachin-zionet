using Manager.Constants;
using Manager.Services;
using Manager.Models;

namespace Manager.Endpoints;

public static class AiEndpoints
{
    private sealed class QuestionEndpoint { }
    private sealed class AnswerEndpoint { }
    private sealed class PubSubEndpoint { }

    public static void MapAiEndpoints(this WebApplication app)
    {
        app.MapPost("/ai/question",
            async (AiRequestModel dto, IAiGatewayService aiService, ILogger<QuestionEndpoint> log, CancellationToken ct) =>
            {
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
            async (string id, IAiGatewayService ai, ILogger<AnswerEndpoint> log, CancellationToken ct) =>
            {
                try
                {
                    var ans = await ai.GetAnswerAsync(id, ct);
                    if (ans is null)
                    {
                        log.LogDebug("Answer for {Id} not ready", id);
                        return Results.NotFound(new { error = "Answer not ready" });
                    }

                    return Results.Ok(new { id, answer = ans });
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error retrieving answer {Id}", id);
                    return Results.Problem("AI answer retrieval failed");
                }
            });

        app.MapPost($"/ai/{TopicNames.AiToManager}",
            async (AiResponseModel msg, IAiGatewayService ai, ILogger<PubSubEndpoint> log, CancellationToken ct) =>
            {
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