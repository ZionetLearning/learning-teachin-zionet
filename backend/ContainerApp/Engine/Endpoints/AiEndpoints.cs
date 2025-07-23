using Dapr.Client;
using Engine.Constants;
using Engine.Models;
using Engine.Services;

namespace Engine.Endpoints;

public static class AiEndpoints
{
    public static void MapAiEndpoints(this WebApplication app)
    {
        app.MapPost($"/{QueueNames.ManagerToAi}-input",
            async (AiRequestModel req,
                   IChatAiService aiService,
                   IAiReplyPublisher publisher,
                   ILoggerFactory lf,
                   CancellationToken ct) =>
            {
                var log = lf.CreateLogger("AiEndpoints.ManagerToAi");
                log.LogInformation("Received AI question {Id} from manager", req.Id);

                var response = await aiService.ProcessAsync(req, ct);

                await publisher.PublishAsync(response, req.ReplyToTopic, ct);

                return Results.Ok();
            });
    }
}