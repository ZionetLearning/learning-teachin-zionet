using AutoMapper;
using Dapr.Client;
using Manager.Constants;
using Manager.Models;


namespace Manager.Services;

public class ManagerService : IManagerService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ManagerService> _logger;
    private readonly DaprClient _daprClient;
    private readonly IMapper _mapper;

    public ManagerService(IConfiguration configuration, 
        ILogger<ManagerService> logger, 
        DaprClient daprClient,
        IMapper mapper)
    {
        _configuration = configuration;
        _logger = logger;
        _daprClient = daprClient;
        _mapper = mapper;
    }

    public async Task<TaskModel?> GetTaskAsync(int id)
    {
        _logger.LogInformation("Inside:{method}", nameof(GetTaskAsync));
        try
        {
            var task = await _daprClient.InvokeMethodAsync<TaskModel>(
                HttpMethod.Get,
                "accessor",
                $"task/{id}"
            );
            if (task is null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found in Accessor service.", id);
                return null;
            }
            _logger.LogInformation("Retrieved task with ID {TaskId} from Accessor service.", id);
            return task;
        }
        
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get task with ID {TaskId} from Accessor service.", id);
            throw;
        }
    }

    public async Task<(bool success, string message)> ProcessTaskAsync(TaskModel task)
    {
        _logger.LogInformation("Inside: {method}", nameof(ProcessTaskAsync));
        if (task is null)
        {
            _logger.LogWarning("Null task received for processing");
            return (false, "Task is null");
        }

        try
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(task);
            _logger.LogDebug("Serialized task payload: {Payload}", payload);

            await _daprClient.InvokeBindingAsync(QueueNames.ManagerToEngine, "create", task);
            _logger.LogInformation("Task {Id} sent to Engine via binding '{Binding}'", task.Id, QueueNames.ManagerToEngine);

            return (true, "sent to engine");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task {Id} to Engine", task.Id);
            return (false, "Failed to send to Engine");
        }
    }

    public async Task<bool> UpdateTaskName(int id, string newTaskName)
    {
        _logger.LogInformation("Inside {method}", nameof(UpdateTaskName));
        try
        {
            if (id <= 0 || string.IsNullOrEmpty(newTaskName))
            {
                _logger.LogError("Invalid input: id or task name is null or empty.");
                return false;
            }

            await _daprClient.InvokeBindingAsync(
                QueueNames.TaskUpdate,
                "create",
                new
                {
                    Id = id,
                    Name = newTaskName
                });

            _logger.LogInformation("Task name update sent to queue.");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inside the UpdateUserEmail");
            return false;
        }
    }

    public async Task<bool> DeleteTask(int id)
    {
        _logger.LogInformation("Inside {method}", nameof(DeleteTask));
        try
        {
            if (id <= 0)
            {
                _logger.LogError("Invalid input.");
                return false;
            }

            await _daprClient.InvokeMethodAsync(
                HttpMethod.Delete,
                "accessor",
                $"task/{id}");

            _logger.LogInformation($"{nameof(DeleteTask)}: task with id: {id} has been deleted");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inside the DeleteUser");
            return false;
        }
    }

}
