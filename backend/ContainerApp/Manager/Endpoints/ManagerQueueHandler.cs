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
using Manager.Models.Words;
using Manager.Models.UserGameConfiguration;
using Manager.Services.Clients.Accessor.Interfaces;

namespace Manager.Endpoints;

public class ManagerQueueHandler : RoutedQueueHandler<Message, MessageAction>
{
    private readonly INotificationService _notificationService;
    private readonly IGameAccessorClient _gameAccessorClient;
    private readonly ILogger<ManagerQueueHandler> _logger;
    protected override MessageAction GetAction(Message message) => message.ActionName;

    protected override void Configure(RouteBuilder routes) => routes
        .On(MessageAction.NotifyUser, HandleNotifyUserAsync)
        .On(MessageAction.ProcessingChatMessage, HandleAIChatAnswerAsync)
        .On(MessageAction.GenerateSentences, HandleGenerateAnswer)
        .On(MessageAction.GenerateSplitSentences, HandleGenerateSplitAnswer)
        .On(MessageAction.GenerateWordExplain, HandleWordExplainAnswer);

    public ManagerQueueHandler(
        ILogger<ManagerQueueHandler> logger,
        INotificationService notificationService,
        IGameAccessorClient gameAccessorClient) : base(logger)
    {
        _notificationService = notificationService;
        _logger = logger;
        _gameAccessorClient = gameAccessorClient;
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

            await _notificationService.SendEventAsync(EventType.ChatAiAnswer, userContextMetadata.UserId, chatResponse);

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

            var gameTypeString = generatedResponse.Sentences.FirstOrDefault()?.GameType ?? "typingPractice";
            var gameType = Enum.TryParse<GameName>(gameTypeString, ignoreCase: true, out var parsedGameType)
     ? parsedGameType
  : Manager.Models.UserGameConfiguration.GameName.TypingPractice;

            var dto = new GeneratedSentenceDto
            {
                StudentId = Guid.Parse(userId),
                GameType = gameType,
                Difficulty = Enum.TryParse<Models.Games.Difficulty>(generatedResponse.Sentences.FirstOrDefault()?.Difficulty, ignoreCase: true, out var difficulty)
                    ? difficulty
                    : Manager.Models.Games.Difficulty.Easy,
                Sentences = [.. generatedResponse.Sentences
                    .Select(s => new GeneratedSentenceItem
                    {
                        Text = s.Text,
                        CorrectAnswer = [s.Text],
                        Nikud = s.Nikud
                    })]
            };

            var result = await _gameAccessorClient.SaveGeneratedSentencesAsync(dto, cancellationToken);

            var response = new SentenceGenerationResponse
            {
                RequestId = generatedResponse.RequestId,
                Sentences = result.Select(r => new GeneratedSentenceResultItem
                {
                    ExerciseId = r.ExerciseId,
                    Text = r.Text,
                    Words = r.Words,
                    Difficulty = r.Difficulty,
                    Nikud = r.Nikud
                }).ToList()
            };

            await _notificationService.SendEventAsync(EventType.SentenceGeneration, userId, response);
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
                GameType = Enum.Parse<GameName>("WordOrder"),
                Difficulty = Enum.TryParse<Models.Games.Difficulty>(generatedResponse.Sentences.FirstOrDefault()?.Difficulty, ignoreCase: true, out var difficulty)
                ? difficulty
                : Manager.Models.Games.Difficulty.Easy,
                Sentences = [.. split.Sentences
                .Select(s => new GeneratedSentenceItem
                {
                    Text = s.Text,
                    CorrectAnswer = s.Words,
                    Nikud = s.Nikud
                })]
            };

            var result = await _gameAccessorClient.SaveGeneratedSentencesAsync(dto, cancellationToken);

            var response = new SentenceGenerationResponse
            {
                RequestId = generatedResponse.RequestId,
                Sentences = result.Select(r => new GeneratedSentenceResultItem
                {
                    ExerciseId = r.ExerciseId,
                    Text = r.Text,
                    Words = r.Words,
                    Difficulty = r.Difficulty,
                    Nikud = r.Nikud
                }).ToList()
            };

            await _notificationService.SendEventAsync(
                EventType.SplitSentenceGeneration,
                userId,
                response
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
    public async Task HandleWordExplainAnswer(
    Message message,
    IReadOnlyDictionary<string, string>? metadata,
    Func<Task> renewLock,
    CancellationToken cancellationToken)
    {
        try
        {
            var explainResponse = message.Payload.Deserialize<WordExplainResponse>();
            if (explainResponse is null)
            {
                _logger.LogError("Payload deserialization returned null for word explanation.");
                throw new NonRetryableException("Payload deserialization returned null for word explanation.");
            }

            if (!message.Metadata.HasValue)
            {
                _logger.LogWarning("Metadata is missing for word explanation action");
                throw new NonRetryableException("User metadata is required for word explanation action.");
            }

            var userId = JsonSerializer.Deserialize<string>(message.Metadata.Value);
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("UserId is null or empty for word explanation action");
                throw new NonRetryableException("User metadata is required for word explanation action.");
            }

            if (!ValidationExtensions.TryValidate(explainResponse, out var validationErrors))
            {
                _logger.LogWarning("Validation failed for {Model}: {Errors}",
                    nameof(WordExplainResponse), validationErrors);
                throw new NonRetryableException(
                    $"Validation failed for {nameof(WordExplainResponse)}: {string.Join("; ", validationErrors)}");
            }

            _logger.LogInformation("Sending WordExplain event to user {UserId}", userId);

            await _notificationService.SendEventAsync(EventType.WordExplain, userId, explainResponse);

            _logger.LogInformation("Word explanation sent successfully to user {UserId}", userId);
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON for {Action}", message.ActionName);
            throw new NonRetryableException("Invalid JSON response.", ex);
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Operation cancelled while processing message {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Error processing word explanation response");
            throw new RetryableException("Transient error while processing word explanation.", ex);
        }
    }
}
