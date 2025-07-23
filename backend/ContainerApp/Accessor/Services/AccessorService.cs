using Accessor.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;



namespace Accessor.Services;
    public class AccessorService : IAccessorService
    {
        private readonly ILogger<AccessorService> _logger;
        private static readonly Dictionary<int, TaskModel> _store = new();
    private readonly AccessorDbContext _dbContext;

    public AccessorService(AccessorDbContext dbContext, ILogger<AccessorService> logger)
        {
        _dbContext = dbContext;
            _logger = logger;
        }

    public async Task InitializeAsync()
        {
        _logger.LogInformation("Initializing DB...");

        try
        {
            _logger.LogInformation("Applying EF Core migrations...");
            await _dbContext.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed.");
        }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Failed to connect to PostgreSQL during startup.");
        }
            }


    public async Task<bool> UpdateTaskNameAsync(int taskId, string newName)
    {
        try
        {
            var task = await _dbContext.Tasks.FindAsync(taskId);
            if (task == null) return false;

            task.Name = newName;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update task name.");
            return false;
        }
    }


    public async Task<bool> DeleteTaskAsync(int taskId)
        {
        try
            {
            var task = await _dbContext.Tasks.FindAsync(taskId);
            if (task == null) return false;

            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();
            return true;
            }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete task.");
            return false;
        }
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
