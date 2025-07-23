using Accessor.Models;

namespace Accessor.Services
{
    public class AccessorService : IAccessorService
    {
        private readonly ILogger<AccessorService> _logger;
        private static readonly Dictionary<int, TaskModel> _store = new();

        public AccessorService(ILogger<AccessorService> logger)
        {
            _logger = logger;
        }

        public Task<TaskModel?> GetTaskByIdAsync(int id)
        {
            if (_store.TryGetValue(id, out var task))
            {
                _logger.LogDebug("Task found in store: {Id} - {Name}", task.Id, task.Name);
                return Task.FromResult<TaskModel?>(task);
            }

            _logger.LogInformation("Task with ID {Id} does not exist in the store", id);
            return Task.FromResult<TaskModel?>(null);
        }

        public Task SaveTaskAsync(TaskModel task)
        {
            if (task is null)
            {
                _logger.LogWarning("Received null task to save");
                throw new ArgumentNullException(nameof(task));
            }

            _store[task.Id] = task;
            _logger.LogInformation("Stored task: {Id} - {Name}", task.Id, task.Name);
            return Task.CompletedTask;
        }
    }
}
