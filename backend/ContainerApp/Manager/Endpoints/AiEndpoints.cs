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
        try
        {
            var ans = await aiService.GetAnswerAsync(id, ct);
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
    }


    private static async Task<IResult> QuestionAsync(
        [FromBody] AiRequestModel dto,
        [FromServices] IAiGatewayService aiService,
        [FromServices] ILogger<QuestionEndpoint> log,
        CancellationToken ct)
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
    }


    private static async Task<IResult> PubSubAsync(
        [FromBody] AiResponseModel msg,
        [FromServices] IAiGatewayService aiService,
        [FromServices] ILogger<PubSubEndpoint> log,
        CancellationToken ct)
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