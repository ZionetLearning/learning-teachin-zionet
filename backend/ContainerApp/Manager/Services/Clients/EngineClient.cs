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

    public async Task<(bool success, string message)> ProcessTaskAsync(TaskModel task)
    {
        _logger.LogInformation(
            "Inside: {method} in {class}",
            nameof(ProcessTaskAsync),
            nameof(EngineClient)
        );

        try
        {
            await _daprClient.InvokeBindingAsync(QueueNames.ManagerToEngine, "create", task);

            _logger.LogDebug(
                "Task {TaskId} sent to Engine via binding '{Binding}'",
                task.Id,
                QueueNames.ManagerToEngine
            );
            return (true, "sent to engine");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task {TaskId} to Engine", task.Id);
            throw;
        }
    }
}
