using AutoMapper;
using Manager.Models;
using Manager.Services.Clients;

namespace Manager.Services;

public class ManagerService : IManagerService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ManagerService> _logger;
    private readonly IAccessorClient _accessorClient;
    private readonly IEngineClient _engineClient;
    private readonly IMapper _mapper;

    public ManagerService(IConfiguration configuration,
        ILogger<ManagerService> logger,
        IAccessorClient accessorClient,
        IEngineClient engineClient,
        IMapper mapper)
    {
        this._configuration = configuration;
        this._logger = logger;
        this._accessorClient = accessorClient;
        this._engineClient = engineClient;
        this._mapper = mapper;
    }

    public async Task<TaskModel?> GetTaskAsync(int id)
    {
        this._logger.LogDebug("Inside: {MethodName}", nameof(GetTaskAsync));

        if (id <= 0)
        {
            this._logger.LogWarning("Invalid task ID provided: {TaskId}", id);
            return null;
        }

        try
        {
            var task = await this._accessorClient.GetTaskAsync(id);
            if (task != null)
            {
                this._logger.LogDebug("Successfully retrieved task {TaskId}", id);
            }
            else
            {
                this._logger.LogDebug("Task {TaskId} not found", id);
            }

            return task;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred while getting task {TaskId}", id);
            throw;
        }
    }

    public async Task<(bool success, string message)> ProcessTaskAsync(TaskModel task)
    {
        this._logger.LogDebug("Inside: {MethodName}", nameof(ProcessTaskAsync));

        if (task is null)
        {
            this._logger.LogWarning("Null task received for processing");
            return (false, "Task is null");
        }

        if (string.IsNullOrWhiteSpace(task.Name))
        {
            this._logger.LogWarning("Task {TaskId} has invalid name", task.Id);
            return (false, "Task name is required");
        }

        if (string.IsNullOrWhiteSpace(task.Payload))
        {
            this._logger.LogWarning("Task {TaskId} has invalid payload", task.Id);
            return (false, "Task payload is required");
        }

        try
        {
            this._logger.LogDebug("Processing task {TaskId} with name '{TaskName}'", task.Id, task.Name);
            var result = await this._engineClient.ProcessTaskAsync(task);
            if (result.success)
            {
                this._logger.LogDebug("Task {TaskId} successfully processed", task.Id);
            }
            else
            {
                this._logger.LogDebug("Task {TaskId} processing failed: {Message}", task.Id, result.message);
            }

            return result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred while processing task {TaskId}", task.Id);
            return (false, "Failed to send to Engine");
        }
    }

    public async Task<bool> UpdateTaskName(int id, string newTaskName)
    {
        this._logger.LogDebug("Inside: {MethodName}", nameof(UpdateTaskName));

        if (id <= 0)
        {
            this._logger.LogWarning("Invalid task ID provided for update: {TaskId}", id);
            return false;
        }

        if (string.IsNullOrWhiteSpace(newTaskName))
        {
            this._logger.LogWarning("Invalid task name provided for update: '{TaskName}'", newTaskName);
            return false;
        }

        if (newTaskName.Length > 100)
        {
            this._logger.LogWarning("Task name too long for task {TaskId}: {Length} characters", id, newTaskName.Length);
            return false;
        }

        try
        {
            this._logger.LogInformation("Updating task {TaskId} name to '{NewTaskName}'", id, newTaskName);
            var result = await this._accessorClient.UpdateTaskName(id, newTaskName);
            if (result)
            {
                this._logger.LogInformation("Task {TaskId} name successfully updated", id);
            }
            else
            {
                this._logger.LogWarning("Failed to update task {TaskId} name", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred while updating task {TaskId} name", id);
            return false;
        }
    }

    public async Task<bool> DeleteTask(int id)
    {
        this._logger.LogDebug("Inside: {MethodName}", nameof(DeleteTask));
        if (id <= 0)
        {
            this._logger.LogWarning("Invalid task ID provided for deletion: {TaskId}", id);
            return false;
        }

        try
        {
            this._logger.LogInformation("Attempting to delete task {TaskId}", id);
            var result = await this._accessorClient.DeleteTask(id);
            if (result)
            {
                this._logger.LogInformation("Task {TaskId} successfully deleted", id);
            }
            else
            {
                this._logger.LogWarning("Failed to delete task {TaskId}", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred while deleting task {TaskId}", id);
            return false;
        }
    }
}
