using AutoMapper;
using Manager.Models;
using Manager.Services.Clients;
using Dapr.Client;

namespace Manager.Services;

public class ManagerService : IManagerService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ManagerService> _logger;
    private readonly IAccessorClient _accessorClient;
    private readonly IEngineClient _engineClient;
    private readonly IMapper _mapper;
    private readonly DaprClient _dapr;

    public ManagerService(IConfiguration configuration,
        ILogger<ManagerService> logger,
        IAccessorClient accessorClient,
        IEngineClient engineClient,
        IMapper mapper,
        DaprClient dapr)
    {
        _configuration = configuration;
        _logger = logger;
        _accessorClient = accessorClient;
        _engineClient = engineClient;
        _mapper = mapper;
        _dapr = dapr;
    }

    public async Task<TaskModel?> GetTaskAsync(int id)
    {
        _logger.LogDebug("Inside: {MethodName}", nameof(GetTaskAsync));

        if (id <= 0)
        {
            _logger.LogWarning("Invalid task ID provided: {TaskId}", id);
            return null;
        }

        try
        {
            var task = await _accessorClient.GetTaskAsync(id);
            if (task != null)
            {

                _logger.LogDebug("Successfully retrieved task {TaskId}", id);
            }
            else
            {
                _logger.LogDebug("Task {TaskId} not found", id);
            }

            return task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting task {TaskId}", id);
            throw;
        }
    }

    public async Task<(bool success, string message, int? taskId)> ProcessTaskAsync(TaskModel task, string idempotencyKey)
    {
        _logger.LogDebug("Inside: {MethodName}", nameof(ProcessTaskAsync));

        if (task == null)
        {
            return (false, "Task is null", null);
        }

        if (string.IsNullOrWhiteSpace(task.Name))
        {
            return (false, "Task name is required", task.Id);
        }

        if (string.IsNullOrWhiteSpace(task.Payload))
        {
            return (false, "Task payload is required", task.Id);
        }

        try
        {
            _logger.LogDebug("Processing task {TaskId} with name '{TaskName}'", task.Id, task.Name);

            var (success, message, taskId) = await _engineClient.ProcessTaskAsync(task, idempotencyKey);

            //If the Accessor already processed this task, reflect that back to the caller
            if (!success && message == "AlreadyProcessed")
            {
                return (false, "AlreadyProcessed", taskId);
            }

            return (true, message, taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing task {TaskId}", task.Id);
            return (false, "Failed to send to Engine", task.Id);
        }
    }

    public async Task<bool> UpdateTaskName(int id, string newTaskName)
    {
        _logger.LogDebug("Inside: {MethodName}", nameof(UpdateTaskName));

        if (id <= 0)
        {
            _logger.LogWarning("Invalid task ID provided for update: {TaskId}", id);
            return false;
        }

        if (string.IsNullOrWhiteSpace(newTaskName))
        {
            _logger.LogWarning("Invalid task name provided for update: '{TaskName}'", newTaskName);
            return false;
        }

        if (newTaskName.Length > 100)
        {
            _logger.LogWarning("Task name too long for task {TaskId}: {Length} characters", id, newTaskName.Length);
            return false;
        }

        try
        {
            _logger.LogInformation("Updating task {TaskId} name to '{NewTaskName}'", id, newTaskName);
            var result = await _accessorClient.UpdateTaskName(id, newTaskName);
            if (result)
            {
                _logger.LogInformation("Task {TaskId} name successfully updated", id);
            }
            else
            {
                _logger.LogWarning("Failed to update task {TaskId} name", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating task {TaskId} name", id);
            return false;
        }
    }

    public async Task<bool> DeleteTask(int id)
    {
        _logger.LogDebug("Inside: {MethodName}", nameof(DeleteTask));
        if (id <= 0)
        {
            _logger.LogWarning("Invalid task ID provided for deletion: {TaskId}", id);
            return false;
        }

        try
        {
            _logger.LogInformation("Attempting to delete task {TaskId}", id);
            var result = await _accessorClient.DeleteTask(id);
            if (result)
            {
                _logger.LogInformation("Task {TaskId} successfully deleted", id);
            }
            else
            {
                _logger.LogWarning("Failed to delete task {TaskId}", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting task {TaskId}", id);
            return false;
        }
    }
}
