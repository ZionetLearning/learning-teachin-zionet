using Manager.Models;
using Manager.Models.ModelValidation;
using Manager.Services;
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
        log.LogInformation("Inside {Method}", nameof(QuestionAsync));
        if (!ValidationExtensions.TryValidate(dto, out var validationErrors))
        {
            log.LogWarning("Validation failed for {Model}: {Errors}", nameof(AiRequestModel), validationErrors);
            return Results.BadRequest(new { errors = validationErrors });
        }

        try
        {
            var threadId = dto.ThreadId;

            var id = await aiService.SendQuestionAsync(threadId, dto.Question, ct);
            log.LogInformation("Request {Id} (thread {Thread}) accept", id, threadId);
            return Results.Accepted($"/ai/answer/{id}", new { questionId = id, threadId });
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error sending question");
            return Results.Problem("AI question failed");
        }
    }
}