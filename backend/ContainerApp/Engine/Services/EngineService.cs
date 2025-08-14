using AutoMapper;
using Dapr.Client;
using Engine.Messaging;
using Engine.Models;
using Polly;
using System.Text;
using System.Text.Json;

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

    public async Task ProcessTaskAsync(TaskModel task, IDictionary<string, string>? callbackHeaders, CancellationToken ct)
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
            // Create the request (no payload yet)
            var request = _daprClient.CreateInvokeMethodRequest(
                HttpMethod.Post,
                "accessor",
                "task"
            );

            // Serialize the task into JSON
            var json = JsonSerializer.Serialize(task);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add headers if provided
            if (callbackHeaders != null)
            {
                foreach (var header in callbackHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Send request with cancellation
            await _daprClient.InvokeMethodAsync(request, ct);

            _logger.LogInformation("Task {Id} forwarded to the Accessor service", task.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task {Id} to Accessor", task.Id);
            throw;
        }
    }
}