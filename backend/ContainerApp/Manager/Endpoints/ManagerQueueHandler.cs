using Manager.Models;
using Manager.Models.ModelValidation;
using Manager.Services;
using Manager.Messaging;
using System.Text.Json;

namespace Manager.Endpoints;
public class ManagerQueueHandler : IQueueHandler<Message>
{
    private readonly IAiGatewayService _aiService;
    private readonly ILogger<ManagerQueueHandler> _logger;
    private readonly Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>> _handlers;

    public ManagerQueueHandler(
        IAiGatewayService aiService,
        ILogger<ManagerQueueHandler> logger)
    {
        _aiService = aiService;
        _logger = logger;
        _handlers = new Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>>
        {
            [MessageAction.AnswerAi] = HandleAnswerAiAsync,
        };
    }

    public async Task HandleAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        if (_handlers.TryGetValue(message.ActionName, out var handler))
        {
            await handler(message, renewLock, cancellationToken);
        }
        else
        {
            _logger.LogWarning("No handler for action {Action}", message.ActionName);
            throw new NonRetryableException($"No handler for action {message.ActionName}");
        }
    }

    public async Task HandleAnswerAiAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var response = message.Payload.Deserialize<AiResponseModel>();
            if (response is null)
            {
                _logger.LogError("Payload deserialization returned null for AiResponseModel.");
                throw new NonRetryableException("Payload deserialization returned null for AiResponseModel.");
            }

            _logger.LogInformation("Received AI answer {Id} from engine", response!.Id);

            if (!ValidationExtensions.TryValidate(response, out var validationErrors))
            {
                _logger.LogWarning("Validation failed for {Model}: {Errors}",
                    nameof(AiResponseModel), validationErrors);
                throw new NonRetryableException(
                    $"Validation failed for {nameof(AiResponseModel)}: {string.Join("; ", validationErrors)}");
            }

            await _aiService.SaveAnswerAsync(response, cancellationToken);
            _logger.LogInformation("Answer {Id} saved", response.Id);
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON payload for {Action}", message.ActionName);
            throw new NonRetryableException("Invalid JSON payload.", ex);
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Operation cancelled while processing message {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Error saving answer");
            throw new RetryableException("Transient error while saving answer.", ex);
        }
    }
}
