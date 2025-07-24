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
        _dapr = dapr ?? throw new ArgumentNullException(nameof(dapr));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    public async Task PublishAsync(AiResponseModel response, string replyToTopic, CancellationToken ct = default)
    {
        try
        {
            _log.LogInformation("Publishing AI answer {Id} to topic {Topic}", response.Id, replyToTopic);

            await _dapr.PublishEventAsync("pubsub", replyToTopic, response, ct);

            _log.LogDebug("AI answer {Id} published successfully", response.Id);
        }
        catch (Exception ex)
        {
            _log.LogError(ex,
                "Failed to publish AI answer {Id} to topic {Topic}", response.Id, replyToTopic);
            throw;
        }
    }
}