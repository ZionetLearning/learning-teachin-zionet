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

    // add error that the retries will catch them
    public async Task HandleAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        if (_handlers.TryGetValue(message.ActionName, out var handler))
        {
            await handler(message, renewLock, cancellationToken);
        }
        else
        {
            _logger.LogWarning("No handler for action {Action}", message.ActionName);
        }
    }

    public async Task HandleAnswerAiAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var response = message.Payload.Deserialize<AiResponseModel>();
            _logger.LogInformation("Received AI answer {Id} from engine", response!.Id);

            if (!ValidationExtensions.TryValidate(response, out var validationErrors))
            {
                _logger.LogWarning("Validation failed for {Model}: {Errors}",
                    nameof(AiResponseModel), validationErrors);
                return;
            }

            await _aiService.SaveAnswerAsync(response, cancellationToken);
            _logger.LogInformation("Answer {Id} saved", response.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving answer");
            throw; // Let retry policy handle it
        }
    }
}
