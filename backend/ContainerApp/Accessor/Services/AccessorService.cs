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
        this._dbContext = dbContext;
        this._logger = logger;
        this._daprClient = daprClient;
        this._configuration = configuration;
        this._ttl = int.Parse(configuration["TaskCache:TTLInSeconds"] ??
                                                  throw new KeyNotFoundException("TaskCache:TTLInSeconds is not configured"));
    }
    public async Task InitializeAsync()
    {
        this._logger.LogInformation("Initializing DB...");

        try
        {
            this._logger.LogInformation("Applying EF Core migrations...");
            await this._dbContext.Database.MigrateAsync();
            this._logger.LogInformation("Database migration completed.");
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to connect to PostgreSQL during startup.");
            throw;
        }
    }

    public async Task<TaskModel?> GetTaskByIdAsync(int id)
    {
        this._logger.LogInformation("Inside:{Method}", nameof(GetTaskByIdAsync));
        try
        {
            var key = GetTaskCacheKey(id);

            // Try to get from Redis cache
            var stateEntry = await this._daprClient.GetStateEntryAsync<TaskModel>(ComponentNames.StateStore, key);

            if (stateEntry.Value != null)
            {
                this._logger.LogInformation("Task {TaskId} found in Redis cache (ETag: {ETag})", id, stateEntry.ETag);
                return stateEntry.Value;
            }

            // If not found in cache, fetch from database
            var task = await this._dbContext.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                this._logger.LogWarning("Task with ID {TaskId} not found.", id);
                return null;
            }

            // Save the task to Redis cache with ETag, if its not already cached
            stateEntry.Value = task;

            await this._daprClient.SaveStateAsync(
                storeName: ComponentNames.StateStore,
                key: key,
                value: task,
                metadata: new Dictionary<string, string>
                {
                    { "ttlInSeconds", this._ttl.ToString() }
                }
            );

            this._logger.LogInformation("Fetched task {TaskId} from DB and cached", id);
            return task;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving task with ID {TaskId}", id);
            throw;
        }
    }

    public async Task CreateTaskAsync(TaskModel task)
    {
        this._logger.LogInformation("Inside:{Method}", nameof(CreateTaskAsync));
        try
        {
            // Check if the id already exists in the DB
            var checkId = await this._dbContext.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == task.Id);

            if (checkId != null)
            {
                this._logger.LogWarning("Task with ID {TaskId} already exists in the DB.", task.Id);
                return;
            }

            // Add task to the database
            this._dbContext.Tasks.Add(task);
            await this._dbContext.SaveChangesAsync();

            var key = GetTaskCacheKey(task.Id);

            // Check if the task already exists in Redis cache
            var stateEntry = await this._daprClient.GetStateEntryAsync<TaskModel>(ComponentNames.StateStore, key);

            // If it exists, log a warning and overwrite it
            if (stateEntry.Value != null)
            {
                this._logger.LogWarning("Task {TaskId} already present in cache before creation. Overwriting.", task.Id);
            }

            // Store the newly created task in Redis cache
            stateEntry.Value = task;

            await this._daprClient.SaveStateAsync(
               storeName: ComponentNames.StateStore,
               key: key,
               value: task,
               metadata: new Dictionary<string, string>
               {
                    { "ttlInSeconds", this._ttl.ToString() }
               });
            this._logger.LogInformation("Successfully cached task {TaskId} with TTL {TTL}s", task.Id, this._ttl);
        }
        catch (DbUpdateException dbEx)
        {
            this._logger.LogError(dbEx, "Failed to save task {TaskId} to the database due to DB error.", task.Id);
            throw;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Unexpected error occurred while saving task {TaskId}", task.Id);
            throw;
        }
    }

    public async Task<bool> UpdateTaskNameAsync(int taskId, string newName)
    {
        this._logger.LogInformation("Inside:{Method}", nameof(UpdateTaskNameAsync));
        try
        {
            var task = await this._dbContext.Tasks.FindAsync(taskId);
            if (task == null)
            {
                this._logger.LogWarning("Task {TaskId} not found in DB.", taskId);
                return false;
            }

            task.Name = newName;
            await this._dbContext.SaveChangesAsync();

            var key = GetTaskCacheKey(taskId);

            // Load current state from Redis
            var stateEntry = await this._daprClient.GetStateEntryAsync<TaskModel>(ComponentNames.StateStore, key);

            if (stateEntry.Value == null)
            {
                this._logger.LogWarning("Task {TaskId} not found in Redis cache. Skipping cache update.", taskId);
                return true;
            }

            stateEntry.Value.Name = newName;

            await this._daprClient.SaveStateAsync(
               storeName: ComponentNames.StateStore,
               key: key,
               value: task,
               metadata: new Dictionary<string, string>
               {
                    { "ttlInSeconds", this._ttl.ToString() }
               });
            this._logger.LogInformation("Successfully cached task {TaskId} with TTL {TTL}s", task.Id, this._ttl);
            return true;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to update task name.");
            return false;
        }
    }

    public async Task<bool> DeleteTaskAsync(int taskId)
    {
        this._logger.LogInformation("Inside:{Method}", nameof(DeleteTaskAsync));
        try
        {
            var task = await this._dbContext.Tasks.FindAsync(taskId);
            if (task == null)
            {
                this._logger.LogWarning("Task {TaskId} not found in DB.", taskId);
                return false;
            }
            // Remove task from the database
            this._dbContext.Tasks.Remove(task);
            await this._dbContext.SaveChangesAsync();
            this._logger.LogInformation("Task {TaskId} deleted from DB.", taskId);

            // Remove task from Redis cache
            var key = GetTaskCacheKey(taskId);
            var stateEntry = await this._daprClient.GetStateEntryAsync<TaskModel>(ComponentNames.StateStore, key);

            if (stateEntry.Value == null)
            {
                this._logger.LogWarning("Task {TaskId} not found in Redis cache. Skipping cache deletion.", taskId);
                return true;
            }

            var deleteSuccess = await stateEntry.TryDeleteAsync();

            if (deleteSuccess)
            {
                this._logger.LogInformation("Task {TaskId} removed from Redis cache (ETag: {ETag})", taskId, stateEntry.ETag);
            }
            else
            {
                this._logger.LogWarning("ETag conflict — failed to remove task {TaskId} from Redis cache.", taskId);
            }

            return true;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to delete task.");
            return false;
        }
    }

    private static string GetTaskCacheKey(int taskId) => $"task:{taskId}";

}
