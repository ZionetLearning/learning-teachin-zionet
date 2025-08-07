using Engine.Messaging;
using Engine.Models;
using Engine.Services;

namespace Engine.Endpoints;

public class EngineAiQueueHandler : IQueueHandler<AiRequestModel>
{
    private readonly IChatAiService _aiService;
    private readonly IAiReplyPublisher _publisher;
    private readonly ILogger<EngineAiQueueHandler> _logger;

    public EngineAiQueueHandler(
        IChatAiService aiService,
        IAiReplyPublisher publisher,
        ILogger<EngineAiQueueHandler> logger)
    {
        _aiService = aiService;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task HandleAsync(AiRequestModel message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received AI question {Id} from manager", message.Id);

        try
        {
            if (string.IsNullOrWhiteSpace(message.ThreadId))
            {
                _logger.LogWarning("ThreadId is required.");
                return;
            }

            var response = await _aiService.ProcessAsync(message, cancellationToken);

            await _publisher.SendReplyAsync(response, message.ReplyToQueue, cancellationToken);

            _logger.LogInformation("AI question {Id} processed", message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process AI question {Id}", message.Id);
            throw;
        }
    }
}
