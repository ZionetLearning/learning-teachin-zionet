using System.Text.Json;
using AutoMapper;
using DotQueue;
using Manager.Models.Chat;
using Manager.Models.ModelValidation;
using Manager.Models.Notifications;
using Manager.Models.QueueMessages;
using Manager.Services;

namespace Manager.Endpoints;

public class ManagerSessionQueueHandler : IQueueHandler<SessionQueueMessage>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<ManagerSessionQueueHandler> _logger;
    private readonly IMapper _mapper;
    private readonly Dictionary<MessageSessionAction, Func<SessionQueueMessage, Func<Task>, IReadOnlyDictionary<string, string>?, CancellationToken, Task>> _handlers;

    public ManagerSessionQueueHandler(
        INotificationService notificationService,
        ILogger<ManagerSessionQueueHandler> logger,
        IMapper mapper)
    {
        _notificationService = notificationService;
        _logger = logger;
        _mapper = mapper;
        _handlers = new Dictionary<MessageSessionAction, Func<SessionQueueMessage, Func<Task>, IReadOnlyDictionary<string, string>?, CancellationToken, Task>>
        {
            [MessageSessionAction.ChatStream] = HandleStreamAIChatAnswerAsync,
        };
    }

    public async Task HandleAsync(SessionQueueMessage message, IReadOnlyDictionary<string, string>? metadataCallback, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        if (_handlers.TryGetValue(message.ActionName, out var handler))
        {
            await handler(message, renewLock, metadataCallback, cancellationToken);
            return;
        }

        _logger.LogWarning("No handler for action {Action}", message.ActionName);

        throw new NonRetryableException($"No handler for action {message.ActionName}");
    }

    public async Task HandleStreamAIChatAnswerAsync(SessionQueueMessage message, Func<Task> renewLock, IReadOnlyDictionary<string, string>? metadataCallback, CancellationToken cancellationToken)
    {
        try
        {
            var chatResponse = message.Payload.Deserialize<AIChatStreamResponse>();
            if (chatResponse is null)
            {
                _logger.LogError("Payload deserialization returned null for EngineChatResponse.");
                throw new NonRetryableException("Payload deserialization returned null for EngineChatResponse.");
            }

            UserContextMetadata? metadata = null;
            if (message.Metadata.HasValue)
            {
                metadata = JsonSerializer.Deserialize<UserContextMetadata>(message.Metadata.Value);
            }

            if (metadata is null)
            {
                _logger.LogWarning("Metadata is null for ProcessingChatMessage action");
                throw new NonRetryableException("Chat Metadata is required for ProcessingChatMessage action.");
            }

            if (!ValidationExtensions.TryValidate(chatResponse, out var validationErrors))
            {
                _logger.LogWarning("Validation failed for {Model}: {Errors}",
                    nameof(AIChatResponse), validationErrors);
                throw new NonRetryableException(
                    $"Validation failed for {nameof(AIChatResponse)}: {string.Join("; ", validationErrors)}");
            }

            _logger.LogInformation("Chat frame kind: {FrameKind}", message.Frame);

            if (message.Frame == FrameKind.Last)
            {
                _logger.LogInformation("Final chat response received for request {RequestId}", chatResponse.RequestId);
            }

            var streamEvent = new StreamEvent<AIChatStreamResponse>
            {
                EventType = StreamEventType.ChatAiAnswer,
                Stage = _mapper.Map<StreamEventStage>(message.Frame),
                Payload = chatResponse,
                SequenceNumber = message.Sequence,
                RequestId = message.CorrelationId
            };

            await _notificationService.SendStreamEventAsync(streamEvent, userId: metadata.UserId);
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON chat response for {Action}", message.ActionName);
            throw new NonRetryableException("Invalid JSON chat response.", ex);
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Operation cancelled while processing message {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Error processing AI chat answer");
            throw new RetryableException("Transient error while processing AI chat answer.", ex);
        }
    }
}
