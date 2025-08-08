using Accessor.Constants;
using Accessor.DB;
using Accessor.Models;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;
public class AccessorService : IAccessorService
{
    private readonly ILogger<AccessorService> _logger;
    private readonly AccessorDbContext _dbContext;
    private readonly DaprClient _daprClient;
    private readonly IConfiguration _configuration;
    private readonly int _ttl;

    public AccessorService(AccessorDbContext dbContext,
        ILogger<AccessorService> logger,
        DaprClient daprClient,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _logger = logger;
        _daprClient = daprClient;
        _configuration = configuration;
        _ttl = int.Parse(configuration["TaskCache:TTLInSeconds"] ??
                                                  throw new KeyNotFoundException("TaskCache:TTLInSeconds is not configured"));
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

            // Try to get from Redis cache
            var stateEntry = await _daprClient.GetStateEntryAsync<TaskModel>(ComponentNames.StateStore, key);

            if (stateEntry.Value != null)
            {
                _logger.LogInformation("Task {TaskId} found in Redis cache (ETag: {ETag})", id, stateEntry.ETag);
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

            // Save the task to Redis cache with ETag, if its not already cached
            stateEntry.Value = task;

            await _daprClient.SaveStateAsync(
                storeName: ComponentNames.StateStore,
                key: key,
                value: task,
                metadata: new Dictionary<string, string>
                {
                    { "ttlInSeconds", _ttl.ToString() }
                }
            );

            _logger.LogInformation("Fetched task {TaskId} from DB and cached", id);
            return task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task with ID {TaskId}", id);
            throw;
        }
    }

    public async Task<(bool success, string message, int? taskId)> CreateTaskAsync(TaskModel task, string idempotencyKey = "")
    {
        _logger.LogInformation("Inside: {Method}", nameof(CreateTaskAsync));

        try
        {
            // Check if the request with the same idempotency key was already processed
            var existing = await _daprClient.GetStateAsync<TaskModel>(ComponentNames.StateStore, idempotencyKey);
            if (existing != null)
            {
                _logger.LogWarning("Duplicate request detected for Idempotency-Key: {Key}. Task ID: {TaskId}", idempotencyKey, existing.Id);
                return (false, "AlreadyProcessed", existing.Id);
            }

            // Check if task with same ID already exists in DB (extra guard)
            var checkId = await _dbContext.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == task.Id);

            if (checkId != null)
            {
                _logger.LogWarning("Task with ID {TaskId} already exists in the DB.", task.Id);
                return (false, "AlreadyProcessed", task.Id);
            }

            // Add task to database
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();

            // Save idempotency key → task in Dapr
            await _daprClient.SaveStateAsync(
                storeName: ComponentNames.StateStore,
                key: idempotencyKey,
                value: task,
                metadata: new Dictionary<string, string>
                {
                    { "ttlInSeconds", _ttl.ToString() }
                });

            _logger.LogInformation("Stored idempotency record for key {Key}", idempotencyKey);

            // Cache task for faster future retrieval (optional)
            var taskCacheKey = GetTaskCacheKey(task.Id);
            await _daprClient.SaveStateAsync(
                storeName: ComponentNames.StateStore,
                key: taskCacheKey,
                value: task,
                metadata: new Dictionary<string, string>
                {
                    { "ttlInSeconds", _ttl.ToString() }
                });

            _logger.LogInformation("Successfully cached task {TaskId} with TTL {TTL}s", task.Id, _ttl);

            return (true, "Saved", task.Id);
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
        _logger.LogInformation("Inside:{Method}", nameof(UpdateTaskNameAsync));
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
            var stateEntry = await _daprClient.GetStateEntryAsync<TaskModel>(ComponentNames.StateStore, key);

            if (stateEntry.Value == null)
            {
                _logger.LogWarning("Task {TaskId} not found in Redis cache. Skipping cache update.", taskId);
                return true;
            }

            stateEntry.Value.Name = newName;

            await _daprClient.SaveStateAsync(
               storeName: ComponentNames.StateStore,
               key: key,
               value: task,
               metadata: new Dictionary<string, string>
               {
                    { "ttlInSeconds", _ttl.ToString() }
               });
            _logger.LogInformation("Successfully cached task {TaskId} with TTL {TTL}s", task.Id, _ttl);
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
            var stateEntry = await _daprClient.GetStateEntryAsync<TaskModel>(ComponentNames.StateStore, key);

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
