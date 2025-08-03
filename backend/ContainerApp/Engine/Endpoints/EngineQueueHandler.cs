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
        this._engine = engine;
        this._logger = logger;
    }

    public async Task HandleAsync(TaskModel message, CancellationToken cancellationToken)
    {
        try
        {
            this._logger.LogDebug("Processing task {Id}", message.Id);
            await this._engine.ProcessTaskAsync(message, cancellationToken);
            this._logger.LogInformation("Task {Id} processed", message.Id);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed processing task {Id}", message.Id);
            throw;
        }
    }
}
