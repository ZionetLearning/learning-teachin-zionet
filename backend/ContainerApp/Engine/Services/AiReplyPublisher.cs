using System.Text.Json;
using Dapr.Client;
using Engine.Constants;
using Engine.Models.Chat;
using Engine.Models.QueueMessages;
using Engine.Models.Sentences;

namespace Engine.Services;

public sealed class AiReplyPublisher : IAiReplyPublisher
{
    private readonly DaprClient _dapr;
    private readonly ILogger<AiReplyPublisher> _log;
    private const string BindingOperation = "create";
    private const string CallbackBindingName = $"{QueueNames.ManagerCallbackQueue}-out";
    private const string CallbackBindingSessionName = $"{QueueNames.ManagerCallbackSessionQueue}-out";

    public AiReplyPublisher(DaprClient dapr, ILogger<AiReplyPublisher> log)
    {
        _dapr = dapr ?? throw new ArgumentNullException(nameof(dapr));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    public async Task SendReplyAsync(UserContextMetadata chatMetadata, EngineChatResponse response, CancellationToken ct = default)
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
                ActionName = MessageAction.ProcessingChatMessage,
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
    public async Task SendGeneratedMessagesAsync(string userId, SentenceResponse response, MessageAction action, CancellationToken ct = default)
    {
        if (response is null)
        {
            _log.LogWarning("Response cannot be null.");
            return;
        }

        try
        {

            var payload = JsonSerializer.SerializeToElement(response);

            var messageMetadata = JsonSerializer.SerializeToElement(userId);

            var message = new Message
            {
                ActionName = action,
                Payload = payload,
                Metadata = messageMetadata
            };

            _log.LogInformation("Publishing answer to callback binding {Binding}", CallbackBindingName);

            await _dapr.InvokeBindingAsync(CallbackBindingName, BindingOperation, message, cancellationToken: ct);

            _log.LogDebug("Answer published successfully");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to publish answer");
            throw;
        }
    }

    public async Task SendStreamAsync(
    UserContextMetadata chatMetadata,
    EngineChatStreamResponse chunk,
    CancellationToken ct = default)
    {
        if (chunk is null)
        {
            _log.LogWarning("Stream chunk cannot be null.");
            return;
        }

        using var _ = _log.BeginScope("RequestId: {RequestId}", chunk.RequestId);

        try
        {
            var payload = JsonSerializer.SerializeToElement(chunk);
            var messageMetadata = JsonSerializer.SerializeToElement(chatMetadata);

            var sessionMsg = new SessionQueueMessage
            {
                ActionName = MessageSessionAction.ChatStream,
                Frame = chunk.IsFinal ? FrameKind.Last : FrameKind.Chunk,
                SessionId = chunk.ThreadId.ToString(),
                UserId = chunk.UserId,
                Sequence = chunk.Sequence,
                CorrelationId = chunk.RequestId,
                Payload = JsonSerializer.SerializeToElement(chunk),
                Metadata = JsonSerializer.SerializeToElement(chatMetadata),
                CreatedAt = DateTimeOffset.UtcNow,
                TtlSeconds = 60
            };

            var queueMetadata = new Dictionary<string, string>
            {
                ["SessionId"] = chunk.ThreadId.ToString()
            };

            _log.LogInformation("Publishing stream message to binding {Binding}", CallbackBindingSessionName);
            await _dapr.InvokeBindingAsync(CallbackBindingSessionName, BindingOperation, sessionMsg, queueMetadata, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to publish stream delta");
            throw;
        }
    }
}