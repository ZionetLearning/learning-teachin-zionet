using System.Text.Json;
using Dapr.Client;
using Engine.Constants;
using Engine.Models.Chat;
using Engine.Models.QueueMessages;

namespace Engine.Services;

public sealed class AiReplyPublisher : IAiReplyPublisher
{
    private readonly DaprClient _dapr;
    private readonly ILogger<AiReplyPublisher> _log;
    private const string BindingOperation = "create";
    private const string CallbackBindingName = $"{QueueNames.ManagerCallbackQueue}-out";

    public AiReplyPublisher(DaprClient dapr, ILogger<AiReplyPublisher> log)
    {
        _dapr = dapr ?? throw new ArgumentNullException(nameof(dapr));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    public async Task SendReplyAsync(ChatContextMetadata chatMetadata, EngineChatResponse response, CancellationToken ct = default)
    {
        if (response is null)
        {
            _log.LogWarning("Response cannot be null.");
            return;
        }

        using var _ = _log.BeginScope("RequestId: {RequestId}", response.RequestId);

        try
        {
            if (string.IsNullOrWhiteSpace(response.RequestId))
            {
                _log.LogWarning("Response RequestId is required.");
                return;
            }

            var payload = JsonSerializer.SerializeToElement(response);

            var messageMetadata = JsonSerializer.SerializeToElement(chatMetadata);

            var message = new Message
            {
                ActionName = MessageAction.AnswerAi,
                Payload = payload,
                Metadata = messageMetadata
            };

            var queueMetadata = new Dictionary<string, string>
            {
                ["sessionId"] = response.ThreadId.ToString()
            };

            _log.LogInformation("Publishing AI answer to callback binding {Binding}", CallbackBindingName);

            await _dapr.InvokeBindingAsync(CallbackBindingName, BindingOperation, message, metadata: queueMetadata, cancellationToken: ct);

            _log.LogDebug("AI answer published successfully");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to publish AI answer");
            throw;
        }
    }
}