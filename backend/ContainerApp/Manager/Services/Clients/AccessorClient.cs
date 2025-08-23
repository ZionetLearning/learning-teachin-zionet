using Dapr.Client;
using Manager.Constants;
using System.Collections.ObjectModel;
using Manager.Models;
using System.Net;
using System.Text.Json;

namespace Manager.Services.Clients;

public class AccessorClient(ILogger<AccessorClient> logger, DaprClient daprClient) : IAccessorClient
{
    private readonly ILogger<AccessorClient> _logger = logger;
    private readonly DaprClient _daprClient = daprClient;

    public async Task<TaskModel?> GetTaskAsync(int id, IDictionary<string, string>? metadata = null)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetTaskAsync), nameof(AccessorClient));
        try
        {
            var request = _daprClient.CreateInvokeMethodRequest(HttpMethod.Get, "accessor", $"task/{id}");

            // attach callback headers if any
            if (metadata is not null && metadata.Count > 0)
            {
                foreach (var kv in metadata)
                    request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
            }

            var task = await _daprClient.InvokeMethodAsync<TaskModel?>(request);
            _logger.LogDebug("Received task {TaskId} from Accessor service", id);
            return task;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Task with ID {TaskId} not found (404 from accessor)", id);
            return null; // treat 404 as "not found", not exception
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get task {TaskId} from Accessor service", id);
            throw;
        }
    }

    public async Task<bool> UpdateTaskName(int id, string newTaskName, IDictionary<string, string>? metadata = null)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(UpdateTaskName), nameof(AccessorClient));
        try
        {
            var task = await GetTaskAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Task {TaskId} not found, cannot update name.", id);
                return false;
            }

            var payload = JsonSerializer.SerializeToElement(new
            {
                id = id,
                name = newTaskName,
                payload = ""
            });

            var message = new Message
            {
                ActionName = MessageAction.UpdateTask,
                Payload = payload,
                Metadata = metadata
            };

            if (metadata is not null && metadata.Count > 0)
            {
                await _daprClient.InvokeBindingAsync(
                    $"{QueueNames.AccessorQueue}-out",
                    "create",
                    message,
                    new ReadOnlyDictionary<string, string>(metadata)
                );
            }
            else
            {
                await _daprClient.InvokeBindingAsync(
                    $"{QueueNames.AccessorQueue}-out",
                    "create",
                    message
                );
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task name update to queue for task {TaskId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteTask(int id, IDictionary<string, string>? metadata = null)
    {
        _logger.LogInformation(
            "Inside: {Method} in {Class}",
            nameof(DeleteTask),
            nameof(AccessorClient)
        );
        try
        {
            var request = _daprClient.CreateInvokeMethodRequest(HttpMethod.Delete, "accessor", $"task/{id}");

            if (metadata is not null && metadata.Count > 0)
            {
                foreach (var kv in metadata)
                    request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
            }

            await _daprClient.InvokeMethodAsync(request);
            _logger.LogDebug("Task {TaskId} deletion request sent to Accessor service", id);

            return true;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Task with ID {TaskId} not found for deletion (404 from accessor)", id);
            return false; // treat 404 as "not found", not exception
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inside the DeleteUser");
            throw;
        }
    }
}
