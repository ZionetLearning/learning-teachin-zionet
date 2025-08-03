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

        app.MapPost($"/{TopicNames.ManagerToAi}", ProcessQuestionAsync).WithTopic("pubsub", TopicNames.ManagerToAi);

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
        using (log.BeginScope("Method: {Method}, CorrelationId: {Id}, ReplyTo: {ReplyToTopic}",
                                      nameof(ProcessQuestionAsync), req.Id, req.ReplyToTopic))
        {
            try
            {
                log.LogInformation("Received AI question from Manager.");
                var response = await aiService.ProcessAsync(req, ct);
                await publisher.PublishAsync(response, req.ReplyToTopic, ct);
                log.LogInformation("Processed and published response.");
                return Results.Ok();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to process AI question.");
                return Results.Problem("An error occurred while processing the AI question.");
            }
        }
    }
}