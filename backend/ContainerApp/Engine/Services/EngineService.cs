using Dapr.Client;
using Engine.Constants;
using Engine.Models;

namespace Engine.Services
{
    public class EngineService : IEngineService
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<EngineService> _logger;
        private static readonly List<TaskModel> _log = new();

        public EngineService(DaprClient daprClient, ILogger<EngineService> logger)
        {
            _daprClient = daprClient;
            _logger = logger;
        }

        public Task<List<TaskModel>> GetAllTasksAsync()
        {
            _logger.LogDebug("Returning all {Count} tasks from log", _log.Count);
            return Task.FromResult(_log);
        }

        public async Task ProcessTaskAsync(TaskModel task)
        {
            _logger.LogInformation($"Inside: {nameof(ProcessTaskAsync)}");
            if (task is null)
            {
                _logger.LogWarning("Attempted to process a null task");
                throw new ArgumentNullException(nameof(task), "Task cannot be null");
            }
            _logger.LogInformation("Logged task: {Id} - {Name}", task.Id, task.Name);

            try
            {
                await _daprClient.InvokeBindingAsync(QueueNames.EngineToAccessor, "create", task);
                _logger.LogInformation("Task {Id} forwarded to binding '{Binding}'", task.Id, QueueNames.EngineToAccessor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send task {Id} to Accessor", task.Id);
                throw;
            }
        }
    }
}
