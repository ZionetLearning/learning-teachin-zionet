using Accessor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;


namespace Accessor.Services;
    public class AccessorService : IAccessorService
    {
        private readonly ILogger<AccessorService> _logger;
        private readonly AccessorDbContext _dbContext;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;

    public AccessorService(AccessorDbContext dbContext,
        ILogger<AccessorService> logger,
        IMemoryCache cache,
        MemoryCacheEntryOptions cacheOptions)
        {
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
        _cacheOptions =  cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
    }

    
    public async Task InitializeAsync()
        {
        _logger.LogInformation("Initializing DB...");

        try
        {
            _logger.LogInformation("Applying EF Core migrations...");
            await _dbContext.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed.");

            _logger.LogInformation("Clearing in-memory cache...");
            if (_cache is MemoryCache memCache)
            {
                memCache.Compact(1.0);
            }
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
                // Check if the data exists in cache
                if (_cache.TryGetValue(id, out var obj) && obj is CachedTaskEntry cachedEntry)
                {
                    _logger.LogInformation("Returning task {Id} from cache (ETag = {ETag})", id, cachedEntry.ETag);
                    return cachedEntry.Task;
                }

                var task = await _dbContext.Tasks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found.", id);
                    return null;
                }

                // Cache result with ETag
                var etag = GenerateEtag(task);
                cachedEntry = new CachedTaskEntry { ETag = etag, Task = task };

                // Create a new CachedTaskEntry to store in cache
                _cache.Set(id, cachedEntry, _cacheOptions);

                _logger.LogInformation("Fetched task {Id} from DB and cached with ETag = {ETag}", id, etag);
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
                task.UpdatedAt = DateTime.UtcNow;

                _dbContext.Tasks.Add(task);
                await _dbContext.SaveChangesAsync();

                // Check if the task already exists in the cache
                if (_cache.TryGetValue(task.Id, out var obj) && obj is CachedTaskEntry cachedEntry)
                {
                    _logger.LogWarning("Task {TaskId} already exists in cache. somthing wrong! ",task.Id);
                    throw new InvalidOperationException($"Task with ID {task.Id} already exists in cache.");
                }
        
                var etag = GenerateEtag(task);
                _cache.Set(task.Id, new CachedTaskEntry { Task = task, ETag = etag }, _cacheOptions);

                _logger.LogInformation("Task {TaskId} saved and cached with ETag = {ETag}", task.Id, etag);
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
            if (task == null) return false;

            task.Name = newName;
            task.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Check if the id is inside the cache, and then update the cache data with the new name and ETag
            if (_cache.TryGetValue(task.Id, out var obj) && obj is CachedTaskEntry cachedEntry)
            {
                cachedEntry.Task.Name = newName;
                cachedEntry.Task.UpdatedAt = task.UpdatedAt;
                cachedEntry.ETag = GenerateEtag(task);
                _logger.LogInformation("Updated cached task {TaskId} with new name and ETag.", taskId);
            }
            else
            {
                _logger.LogWarning("Cache miss while updating task {TaskId}. Cache might be stale.", taskId);
                return false;
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
            if (task == null) return false;

            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();
            _cache.Remove(taskId);
            return true;
            }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete task.");
            return false;
        }
    }


    private static string GenerateEtag(TaskModel task)
    {
        return $"W/\"{task.UpdatedAt.Ticks}\"";
    }

}
