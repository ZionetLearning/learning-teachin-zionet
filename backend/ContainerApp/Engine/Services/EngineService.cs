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
    private readonly IRetryPolicyProvider _retryPolicyProvider;
    private readonly IAsyncPolicy<HttpResponseMessage> _httpRetryPolicy;

    public EngineService(DaprClient daprClient,
        ILogger<EngineService> logger,
        IMapper mapper,
        IRetryPolicyProvider retryPolicyProvider)
    {
        _daprClient = daprClient;
        _logger = logger;
        _mapper = mapper;
        _retryPolicyProvider = retryPolicyProvider;
        _httpRetryPolicy = _retryPolicyProvider.CreateHttpPolicy(_logger);
    }

    public async Task ProcessTaskAsync(TaskModel task, CancellationToken ct)
    {
        _logger.LogInformation("Inside {Method}", nameof(ProcessTaskAsync));
        ct.ThrowIfCancellationRequested();
        if (task is null)
        {
            _logger.LogWarning("Attempted to process a null task");
            throw new ArgumentNullException(nameof(task), "Task cannot be null");
        }

        _logger.LogInformation("Logged task: {Id} - {Name}", task.Id, task.Name);

        try
        {
            await _httpRetryPolicy.ExecuteAsync(async () =>
                {
                    await _daprClient.InvokeBindingAsync(QueueNames.EngineToAccessor, "create", task);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                });
            _logger.LogInformation("Task {Id} forwarded to binding '{Binding}'", task.Id, QueueNames.EngineToAccessor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task {Id} to Accessor", task.Id);
            throw;
        }
    }
}