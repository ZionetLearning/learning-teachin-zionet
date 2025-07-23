using Dapr.Client;
using Manager.Constants;
using Manager.Models;

namespace Manager.Services
{
    public class ManagerService : IManagerService
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<ManagerService> _logger;
        private static readonly Dictionary<int, TaskModel> _tasks = new();

        public ManagerService(DaprClient daprClient, ILogger<ManagerService> logger)
        {
            _daprClient = daprClient;
            _logger = logger;
        }

        public Task<TaskModel?> GetTaskAsync(int id)
        {
            if (_tasks.TryGetValue(id, out var task))
            {
                _logger.LogInformation("Fetched task: {Id} - {Name}", task.Id, task.Name);
                return Task.FromResult<TaskModel?>(task);
            }

            _logger.LogWarning("Task with ID {Id} not found", id);
            return Task.FromResult<TaskModel?>(null);
        }

        public async Task<(bool success, string message)> ProcessTaskAsync(TaskModel task)
        {
            if (task is null)
            {
                _logger.LogWarning("Null task received for processing");
                return (false, "Task is null");
            }

            _tasks[task.Id] = task;
            _logger.LogInformation("Manager received task: {Id} - {Name}", task.Id, task.Name);

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
    }
}
