using Manager.Constants;
using Manager.Services;
using Manager.Models;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class AiEndpoints
{
    private sealed class QuestionEndpoint { }
    private sealed class AnswerEndpoint { }
    private sealed class PubSubEndpoint { }

    public static WebApplication MapAiEndpoints(this WebApplication app)
    {

        #region HTTP GET

        app.MapGet("/ai/answer/{id}", AnswerAsync).WithName("Answer");

        #endregion

        #region HTTP POST

        app.MapPost("/ai/question", QuestionAsync).WithName("Question");

        app.MapPost($"/ai/{TopicNames.AiToManager}", PubSubAsync).WithTopic("pubsub", TopicNames.AiToManager);

        #endregion

        return app;
    }

    private static async Task<IResult> AnswerAsync(
        [FromRoute] string id,
        [FromServices] IAiGatewayService aiService,
        [FromServices] ILogger<AnswerEndpoint> log,
        CancellationToken ct)
    {
        using (log.BeginScope("Method: {Method}, QuestionId: {Id}", nameof(AnswerAsync), id))
        {
            try
            {
                var ans = await aiService.GetAnswerAsync(id, ct);
                if (ans is null)
                {
                    log.LogInformation("Answer not ready");
                    return Results.NotFound(new { error = "Answer not ready" });
                }
                log.LogInformation("Answer returned");
                return Results.Ok(new { id, answer = ans });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to retrieve answer");
                return Results.Problem("AI answer retrieval failed");
            }
        }
    }

    private static async Task<IResult> QuestionAsync(
        [FromBody] AiRequestModel dto,
        [FromServices] IAiGatewayService aiService,
        [FromServices] ILogger<QuestionEndpoint> log,
        CancellationToken ct)
    {
        using (log.BeginScope("Method: {Method}, Id: {Id}", nameof(QuestionAsync), dto.Id))
        {
            try
            {
                var id = await aiService.SendQuestionAsync(dto.Question, ct);
                log.LogInformation("Question accepted and sent to processing");
                return Results.Accepted($"/ai/answer/{id}", new { questionId = id });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error sending question");
                return Results.Problem("AI question failed");
            }
        }
    }

    private static async Task<IResult> PubSubAsync(
        [FromBody] AiResponseModel msg,
        [FromServices] IAiGatewayService aiService,
        [FromServices] ILogger<PubSubEndpoint> log,
        CancellationToken ct)
    {
        using (log.BeginScope("Method: {Method}, QuestionId: {Id}", nameof(PubSubAsync), msg.Id))
        {
            try
            {
                await aiService.SaveAnswerAsync(msg, ct);
                log.LogInformation("Answer saved");
                return Results.Ok();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error saving answer");
                return Results.Problem("AI answer handling failed");
            }
        }
    }
}