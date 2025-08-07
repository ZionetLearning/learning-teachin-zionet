using Dapr.Client;
using Manager.Constants;
using Manager.Models;

namespace Manager.Services.Clients;

public class EngineClient : IEngineClient
{
    private readonly ILogger<EngineClient> _logger;
    private readonly DaprClient _daprClient;

    public EngineClient(ILogger<EngineClient> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    public async Task<(bool success, string message, int? taskId)> ProcessTaskAsync(TaskModel task, string idempotencyKey)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(ProcessTaskAsync), nameof(EngineClient));

        try
        {
            // Pass the Idempotency-Key as metadata to the binding
            var metadata = new Dictionary<string, string>
            {
                { "Idempotency-Key", idempotencyKey }
            };

            await _daprClient.InvokeBindingAsync(
                QueueNames.ManagerToEngine,
                "create",
                task,
                metadata
            );

            _logger.LogDebug("Task {TaskId} sent to Engine via binding '{Binding}'", task.Id, QueueNames.ManagerToEngine);
            return (true, "AlreadyProcessed", task.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task {TaskId} to Engine", task.Id);
            return (false, "Engine communication failed", task.Id);
        }
    }
}
