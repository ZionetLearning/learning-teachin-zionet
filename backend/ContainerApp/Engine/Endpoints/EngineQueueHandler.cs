using Engine.Messaging;
using Engine.Models;
using Engine.Services;

namespace Engine.Endpoints;

public class EngineQueueHandler : IQueueHandler<TaskModel>
{
    private readonly IEngineService _engine;
    private readonly ILogger<EngineQueueHandler> _logger;
    public EngineQueueHandler(IEngineService engine, ILogger<EngineQueueHandler> logger)
    {
        _engine = engine;
        _logger = logger;
    }

    public async Task HandleAsync(TaskModel message, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Processing task {Id}", message.Id);
            await _engine.ProcessTaskAsync(message, cancellationToken);
            _logger.LogInformation("Task {Id} processed", message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed processing task {Id}", message.Id);
            throw;
        }
    }
}
