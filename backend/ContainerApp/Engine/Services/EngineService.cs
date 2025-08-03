using AutoMapper;
using Dapr.Client;
using Engine.Constants;
using Engine.Models;

namespace Engine.Services;

public class EngineService : IEngineService
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<EngineService> _logger;
    private readonly IMapper _mapper;

    public EngineService(DaprClient daprClient,
        ILogger<EngineService> logger,
        IMapper mapper)
    {
        this._daprClient = daprClient;
        this._logger = logger;
        this._mapper = mapper;
    }

    public async Task ProcessTaskAsync(TaskModel task)
    {
        this._logger.LogInformation("Inside {Method}", nameof(ProcessTaskAsync));
        if (task is null)
        {
            this._logger.LogWarning("Attempted to process a null task");
            throw new ArgumentNullException(nameof(task), "Task cannot be null");
        }

        this._logger.LogInformation("Logged task: {Id} - {Name}", task.Id, task.Name);

        try
        {
            await this._daprClient.InvokeBindingAsync(QueueNames.EngineToAccessor, "create", task);
            this._logger.LogInformation("Task {Id} forwarded to binding '{Binding}'", task.Id, QueueNames.EngineToAccessor);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to send task {Id} to Accessor", task.Id);
            throw;
        }
    }
}
