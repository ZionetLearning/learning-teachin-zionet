using Dapr.Client;
using Engine.Constants;
using Engine.Models;

namespace Engine.Services;

public sealed class AiReplyPublisher : IAiReplyPublisher
{
    private readonly DaprClient _dapr;
    private readonly ILogger<AiReplyPublisher> _log;

    public AiReplyPublisher(DaprClient dapr, ILogger<AiReplyPublisher> log)
    {
        _dapr = dapr;
        _log = log;
    }

    public async Task PublishAsync(AiResponseModel response, string replyToTopic, CancellationToken ct = default)
    {
        _log.LogInformation("Publishing AI answer {Id} to topic {Topic}", response.Id, replyToTopic);
        await _dapr.PublishEventAsync("pubsub", TopicNames.AiToManager, response, ct);
    }
}