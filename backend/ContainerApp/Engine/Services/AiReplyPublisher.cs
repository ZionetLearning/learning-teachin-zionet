using System.Text.Json;
using Dapr.Client;
using Engine.Models;
using Engine.Models.Chat;

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

    public async Task SendReplyAsync(EngineChatResponse response, string replyToQueue, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(replyToQueue))
            {
                _log.LogWarning("replyToQueue is required.");
                return;
            }

            if (response == null)
            {
                _log.LogWarning("Response cannot be null.");
                return;
            }

            if (string.IsNullOrWhiteSpace(response.RequestId))
            {
                _log.LogWarning("Response Id is required.");
                return;
            }

            var payload = JsonSerializer.SerializeToElement(response);
            var message = new Message
            {
                ActionName = MessageAction.AnswerAi,
                Payload = payload
            };

            _log.LogInformation("Publishing AI answer {Id} to the queue {Topic}", response.RequestId, replyToQueue);

            await _dapr.InvokeBindingAsync(replyToQueue, "create", message, cancellationToken: ct);

            _log.LogDebug("AI answer {Id} published successfully", response.RequestId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex,
                "Failed to publish AI answer {Id} to topic {Topic}", response.RequestId, replyToQueue);
            throw;
        }
    }
}