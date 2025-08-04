using Dapr.Client;
using Manager.Constants;
using Manager.Models;

namespace Manager.Services.Clients;

public class AccessorClient(ILogger<AccessorClient> logger, DaprClient daprClient) : IAccessorClient
{
    private readonly ILogger<AccessorClient> _logger = logger;
    private readonly DaprClient _daprClient = daprClient;

    public async Task<TaskModel?> GetTaskAsync(int id)
    {
        _logger.LogInformation(
            "Inside: {method} in {class}",
            nameof(GetTaskAsync),
            nameof(AccessorClient)
        );
        try
        {
            var task = await _daprClient.InvokeMethodAsync<TaskModel>(
                HttpMethod.Get,
                "accessor",
                $"task/{id}"
            );
            _logger.LogDebug("Received task {TaskId} from Accessor service", id);
            return task;
        }
        catch (InvocationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Accessor service returned 404 for task {TaskId}", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get task {TaskId} from Accessor service", id);
            throw;
        }
    }

    public async Task<bool> UpdateTaskName(int id, string newTaskName)
    {
        _logger.LogInformation(
            "Inside: {method} in {class}",
            nameof(UpdateTaskName),
            nameof(AccessorClient)
        );
        try
        {
            var task = await GetTaskAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Task {TaskId} not found, cannot update name.", id);
                return false;
            }
            await _daprClient.InvokeBindingAsync(
                QueueNames.TaskUpdate,
                "create",
                new TaskNameUpdateModel(id, newTaskName)
            );

            _logger.LogDebug("Task name update request sent to queue for task {TaskId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task name update to queue for task {TaskId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteTask(int id)
    {
        _logger.LogInformation(
            "Inside: {method} in {class}",
            nameof(DeleteTask),
            nameof(AccessorClient)
        );
        try
        {
            await _daprClient.InvokeMethodAsync(HttpMethod.Delete, "accessor", $"task/{id}");
            _logger.LogDebug("Task {TaskId} deletion request sent to Accessor service", id);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inside the DeleteUser");
            throw;
        }
    }
}
