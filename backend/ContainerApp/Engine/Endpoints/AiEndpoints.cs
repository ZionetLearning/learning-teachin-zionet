using Engine.Constants;
using Engine.Models;
using Engine.Services;

namespace Engine.Endpoints;

public static class AiEndpoints
{
    private sealed class ManagerToAiEndpoint { }

    public static void MapAiEndpoints(this WebApplication app)
    {
        app.MapPost($"/{TopicNames.ManagerToAi}",
            async (AiRequestModel req,
                   IChatAiService aiService,
                   IAiReplyPublisher publisher,
                   ILogger<ManagerToAiEndpoint> log,
                   CancellationToken ct) =>
            {
                log.LogInformation("Received AI question {Id} from manager", req.Id);

                var response = await aiService.ProcessAsync(req, ct);

                await publisher.PublishAsync(response, req.ReplyToTopic, ct);

                return Results.Ok();
            })
            .WithTopic("pubsub", TopicNames.ManagerToAi);
        
    }
}