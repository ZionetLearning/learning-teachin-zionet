using Accessor.Models;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;
    public class AccessorService : IAccessorService
    {
        private readonly ILogger<AccessorService> _logger;
        private readonly AccessorDbContext _dbContext;
        private readonly DaprClient _daprClient;
        private const string StateStoreName = "statestore";

    public AccessorService(AccessorDbContext dbContext,
        ILogger<AccessorService> logger,
        DaprClient daprClient)
        {
        _dbContext = dbContext;
        _logger = logger;
        _daprClient = daprClient;
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
            throw;
        }
            }

    public async Task<TaskModel?> GetTaskByIdAsync(int id)
    {
        _logger.LogInformation("Inside:{Method}", nameof(GetTaskByIdAsync));
        try
        {
            var key = GetTaskCacheKey(id);

            // Try to get from Redis via Dapr
            var stateEntry = await _daprClient.GetStateEntryAsync<TaskModel>(StateStoreName, key);

            if (stateEntry.Value != null)
            {
                _logger.LogInformation("Task {Id} found in Redis cache (ETag: {ETag})", id, stateEntry.ETag);
                return stateEntry.Value;
            }

            // If not found in cache, fetch from database
            var task = await _dbContext.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found.", id);
                return null;
            }

            // Save the task to Redis cache with ETag, in the future we need configuration for redis
            stateEntry.Value = task;
            await stateEntry.TrySaveAsync();

            _logger.LogInformation("Fetched task {Id} from DB and cached", id);
            return task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task with ID {TaskId}", id);
            throw;
        }
    }

    public async Task CreateTaskAsync(TaskModel task)
    {
        _logger.LogInformation("Inside:{Method}", nameof(CreateTaskAsync));
        try
        {
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();


            var key = GetTaskCacheKey(task.Id);
            var stateEntry = await _daprClient.GetStateEntryAsync<TaskModel>(StateStoreName, key);

            if (stateEntry.Value != null)
            {
                _logger.LogWarning("Task {TaskId} already exists in Redis cache. Overwriting...", task.Id);
            }

            // Store the newly created task in Redis cache
            stateEntry.Value = task;

            var saveSuccess = await stateEntry.TrySaveAsync();

            if (saveSuccess)
            {
                _logger.LogInformation("Task {TaskId} cached in Redis with ETag: {ETag}", task.Id, stateEntry.ETag);
            }
            else
            {
                _logger.LogWarning("ETag conflict while caching task {TaskId}. Cache may be stale.", task.Id);
            }
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Failed to save task {TaskId} to the database due to DB error.", task.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving task {TaskId}", task.Id);
            throw;
        }
    }

    public async Task<bool> UpdateTaskNameAsync(int taskId, string newName)
    {
        _logger.LogInformation("Inside:{Method}",nameof(UpdateTaskNameAsync));
        try
        {
            var task = await _dbContext.Tasks.FindAsync(taskId);
            if (task == null)
            {
                _logger.LogWarning("Task {TaskId} not found in DB.", taskId);
                return false;
            }

            task.Name = newName;
            await _dbContext.SaveChangesAsync();

            var key = GetTaskCacheKey(taskId);

            // Load current state from Redis
            var stateEntry = await _daprClient.GetStateEntryAsync<TaskModel>(StateStoreName, key);

            if (stateEntry.Value == null)
            {
                _logger.LogWarning("Task {TaskId} not found in Redis cache. Skipping cache update.", taskId);
                return true;
            }

            stateEntry.Value.Name = newName;

            var saveSuccess = await stateEntry.TrySaveAsync();
            if (saveSuccess)
            {
                _logger.LogInformation("Updated task {TaskId} in Redis cache with new name: {NewName} and ETag: {ETag}", taskId, newName, stateEntry.ETag);
            }
            else
            {
                _logger.LogWarning("ETag conflict while updating task {TaskId} in Redis cache. Cache may be stale.", taskId);
            }
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
        _logger.LogInformation("Inside:{Method}", nameof(DeleteTaskAsync));
        try
        {
            var task = await _dbContext.Tasks.FindAsync(taskId);
            if (task == null)
            {
                _logger.LogWarning("Task {TaskId} not found in DB.", taskId);
                return false;
            }
            // Remove task from the database
            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Task {TaskId} deleted from DB.", taskId);

            // Remove task from Redis cache
            var key = GetTaskCacheKey(taskId);
            var stateEntry = await _daprClient.GetStateEntryAsync<TaskModel>(StateStoreName, key);

            if (stateEntry.Value == null)
            {
                _logger.LogWarning("Task {TaskId} not found in Redis cache. Skipping cache deletion.", taskId);
                return true;
            }

            var deleteSuccess = await stateEntry.TryDeleteAsync();

            if (deleteSuccess)
            {
                _logger.LogInformation("Task {TaskId} removed from Redis cache (ETag: {ETag})", taskId, stateEntry.ETag);
            }
            else
            {
                _logger.LogWarning("ETag conflict — failed to remove task {TaskId} from Redis cache.", taskId);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete task.");
            return false;
        }
    }

    private static string GetTaskCacheKey(int taskId) => $"task:{taskId}";

}
