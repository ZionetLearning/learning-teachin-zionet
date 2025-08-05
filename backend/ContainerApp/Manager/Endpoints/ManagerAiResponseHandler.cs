using Manager.Models;
using Manager.Models.ModelValidation;
using Manager.Services;
using Manager.Messaging;

namespace Manager.Endpoints; 
public class ManagerAiResponseHandler : IQueueHandler<AiResponseModel>
{
    private readonly IAiGatewayService _aiService;
    private readonly ILogger<ManagerAiResponseHandler> _logger;

    public ManagerAiResponseHandler(
        IAiGatewayService aiService,
        ILogger<ManagerAiResponseHandler> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    public async Task HandleAsync(AiResponseModel message, CancellationToken ct)
    {
        _logger.LogInformation("Received AI answer {Id} from engine", message.Id);

        if (!ValidationExtensions.TryValidate(message, out var validationErrors))
        {
            _logger.LogWarning("Validation failed for {Model}: {Errors}",
                nameof(AiResponseModel), validationErrors);
            return;
        }

        try
        {
            await _aiService.SaveAnswerAsync(message, ct);
            _logger.LogInformation("Answer {Id} saved", message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving answer {Id}", message.Id);
            throw; // Let retry policy handle it
        }
    }
}
