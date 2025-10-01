using System.Text.Json;
using DotQueue;
using Manager.Helpers;
using Manager.Models.Chat;
using Manager.Models.ModelValidation;
using Manager.Models.Notifications;
using Manager.Models.QueueMessages;
using Manager.Models.Sentences;
using Manager.Services;
using Manager.Services.Clients.Accessor.Models;
using Manager.Services.Clients.Accessor;

namespace Manager.Endpoints;
public class ManagerQueueHandler : RoutedQueueHandler<Message, MessageAction>
{
    private readonly INotificationService _notificationService;
    private readonly IAccessorClient _accessorClient;
    private readonly ILogger<ManagerQueueHandler> _logger;
    protected override MessageAction GetAction(Message message) => message.ActionName;

    protected override void Configure(RouteBuilder routes) => routes
        .On(MessageAction.NotifyUser, HandleNotifyUserAsync)
        .On(MessageAction.ProcessingChatMessage, HandleAIChatAnswerAsync)
        .On(MessageAction.GenerateSentences, HandleGenerateAnswer)
        .On(MessageAction.GenerateSplitSentences, HandleGenerateSplitAnswer);

    public ManagerQueueHandler(
        ILogger<ManagerQueueHandler> logger,
        INotificationService notificationService,
        IAccessorClient accessorClient) : base(logger)
    {
        _notificationService = notificationService;
        _logger = logger;
        _accessorClient = accessorClient;
    }

    public async Task HandleNotifyUserAsync(Message message, IReadOnlyDictionary<string, string>? metadata, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var notification = message.Payload.Deserialize<UserNotification>();
            if (notification is null)
            {
                _logger.LogError("Payload deserialization returned null for UserNotification.");
                throw new NonRetryableException("Payload deserialization returned null for UserNotification.");
            }

            UserContextMetadata? userContextMetadata = null;
            if (message.Metadata.HasValue)
            {
                userContextMetadata = JsonSerializer.Deserialize<UserContextMetadata>(message.Metadata.Value);
            }

            if (userContextMetadata is null)
            {
                _logger.LogWarning("Metadata is null for NotifyUser action");
                throw new NonRetryableException("User Metadata is required for NotifyUser action.");
            }

            if (!ValidationExtensions.TryValidate(notification, out var validationErrors))
            {
                _logger.LogWarning("Validation failed for {Model}: {Errors}",
                    nameof(UserNotification), validationErrors);
                throw new NonRetryableException(
                    $"Validation failed for {nameof(UserNotification)}: {string.Join("; ", validationErrors)}");
            }

            _logger.LogInformation("Processing notification {MessageId} for user {UserId}", userContextMetadata.MessageId, userContextMetadata.UserId);

            await _notificationService.SendNotificationAsync(userContextMetadata.UserId, notification);
            _logger.LogInformation("Notification processed for user {UserId}", userContextMetadata.UserId);
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON notification for {Action}", message.ActionName);
            throw new NonRetryableException("Invalid JSON notification.", ex);
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Operation cancelled while processing message {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Error notifying user");
            throw new RetryableException("Transient error while notifying user.", ex);
        }
    }

    public async Task HandleAIChatAnswerAsync(Message message, IReadOnlyDictionary<string, string>? metadata, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var chatResponse = message.Payload.Deserialize<AIChatResponse>();
            if (chatResponse is null)
            {
                _logger.LogError("Payload deserialization returned null for EngineChatResponse.");
                throw new NonRetryableException("Payload deserialization returned null for EngineChatResponse.");
            }

            UserContextMetadata? userContextMetadata = null;
            if (message.Metadata.HasValue)
            {
                userContextMetadata = JsonSerializer.Deserialize<UserContextMetadata>(message.Metadata.Value);
            }

            if (userContextMetadata is null)
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

            var userEvent = new UserEvent<AIChatResponse>
            {
                EventType = EventType.ChatAiAnswer,
                Payload = chatResponse,
            };

            await _notificationService.SendEventAsync(userEvent.EventType, userContextMetadata.UserId, userEvent.Payload);

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
    public async Task HandleGenerateAnswer(Message message, IReadOnlyDictionary<string, string>? metadata, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var generatedResponse = message.Payload.Deserialize<SentenceResponse>();
            if (generatedResponse is null)
            {
                _logger.LogError("Payload deserialization returned null for sentence generation.");
                throw new NonRetryableException("Payload deserialization returned null for sentence generation.");
            }

            var userId = string.Empty;
            if (message.Metadata.HasValue)
            {
                userId = JsonSerializer.Deserialize<string>(message.Metadata.Value);
            }

            if (userId is null)
            {
                _logger.LogWarning("Metadata is null for sentence generation action");
                throw new NonRetryableException("Chat Metadata is required for sentence generation action.");
            }

            if (!ValidationExtensions.TryValidate(generatedResponse, out var validationErrors))
            {
                _logger.LogWarning("Validation failed for {Model}: {Errors}",
                    nameof(SentenceResponse), validationErrors);
                throw new NonRetryableException(
                    $"Validation failed for {nameof(SentenceResponse)}: {string.Join("; ", validationErrors)}");
            }

            await _notificationService.SendEventAsync(EventType.SentenceGeneration, userId, generatedResponse);

        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON response for {Action}", message.ActionName);
            throw new NonRetryableException("Invalid JSON response.", ex);
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Operation cancelled while processing message {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Error processing answer");
            throw new RetryableException("Transient error while processing answer.", ex);
        }
    }
    public async Task HandleGenerateSplitAnswer(Message message, IReadOnlyDictionary<string, string>? metadata, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var generatedResponse = message.Payload.Deserialize<SentenceResponse>();
            if (generatedResponse is null)
            {
                _logger.LogError("Payload deserialization returned null for sentence generation.");
                throw new NonRetryableException("Payload deserialization returned null for sentence generation.");
            }

            var userId = string.Empty;
            if (message.Metadata.HasValue)
            {
                userId = JsonSerializer.Deserialize<string>(message.Metadata.Value);
            }

            if (userId is null)
            {
                _logger.LogWarning("Metadata is null for sentence generation action");
                throw new NonRetryableException("Chat Metadata is required for sentence generation action.");
            }

            if (!ValidationExtensions.TryValidate(generatedResponse, out var validationErrors))
            {
                _logger.LogWarning("Validation failed for {Model}: {Errors}",
                    nameof(SentenceResponse), validationErrors);
                throw new NonRetryableException(
                    $"Validation failed for {nameof(SentenceResponse)}: {string.Join("; ", validationErrors)}");
            }

            var split = Splitter.Split(generatedResponse);

            if (split.Sentences.Count == 0)
            {
                _logger.LogWarning("No sentences found after splitting for user {UserId}", userId);
                throw new NonRetryableException("No sentences found after splitting.");
            }

            var dto = new GeneratedSentenceDto
            {
                StudentId = Guid.Parse(userId),
                GameType = "wordOrderGame",
                Difficulty = Enum.TryParse<Models.Games.Difficulty>(generatedResponse.Sentences.FirstOrDefault()?.Difficulty, ignoreCase: true, out var difficulty)
                ? difficulty
                : Manager.Models.Games.Difficulty.Easy,
                Sentences = [.. split.Sentences
                .Select(s => new GeneratedSentenceItem
                {
                    Original = s.Original,
                    CorrectAnswer = s.Words,
                    Nikud = s.Nikud
                })]
            };

            var result = await _accessorClient.SaveGeneratedSentencesAsync(dto, cancellationToken);

            await _notificationService.SendEventAsync(
                EventType.SplitSentenceGeneration,
                userId,
                result
                );
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON response for {Action}", message.ActionName);
            throw new NonRetryableException("Invalid JSON response.", ex);
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Operation cancelled while processing message {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Error processing answer");
            throw new RetryableException("Transient error while processing answer.", ex);
        }
    }
}
