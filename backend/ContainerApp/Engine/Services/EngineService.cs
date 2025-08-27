using AutoMapper;
using Dapr.Client;
using Engine.Models;
using Polly;

namespace Engine.Services;

public class EngineService : IEngineService
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<EngineService> _logger;
    private readonly IMapper _mapper;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _httpRetryPolicy;

    public EngineService(DaprClient daprClient,
        ILogger<EngineService> logger,
        IMapper mapper,
        IRetryPolicy retryPolicy)
    {
        _daprClient = daprClient;
        _logger = logger;
        _mapper = mapper;
        _retryPolicy = retryPolicy;
        _httpRetryPolicy = _retryPolicy.CreateHttpPolicy(_logger);
    }

    public async Task ProcessTaskAsync(TaskModel task, CancellationToken ct)
    {
        using var _ = _logger.BeginScope("TaskId: {TaskId}", task.Id);
        _logger.LogInformation("Inside {Method}", nameof(ProcessTaskAsync));
        ct.ThrowIfCancellationRequested();
        if (task is null)
        {
            _logger.LogWarning("Attempted to process a null task");
            throw new ArgumentNullException(nameof(task), "Task cannot be null");
        }

        _logger.LogInformation("Logged task: {Name}", task.Name);
        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Post, "accessor", "tasks-accessor/task", task, ct);

            _logger.LogInformation("Task forwarded to the Accessor service");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task to Accessor");
            throw;
        }
    }
}