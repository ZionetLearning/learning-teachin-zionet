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
        }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Failed to connect to PostgreSQL during startup.");
        }
            }


    public async Task<TaskModel?> GetTaskByIdAsync(int id)
        {
            _logger.LogInformation($"Inside:{nameof(GetTaskByIdAsync)}");
            try
            {

                // Try cache
                if (_cache.TryGetValue(id, out CachedTaskEntry cachedEntry))
                {
                    _logger.LogInformation("Returning task {Id} from cache (ETag = {ETag})", id, cachedEntry!.ETag);
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
                _cache.Set(id, new CachedTaskEntry { Task = task, ETag = etag }, _cacheOptions);

                _logger.LogInformation("Fetched task {Id} from DB and cached with ETag = {ETag}", id, etag);
                return task;
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task with ID {TaskId}", id);
                throw;
            }
        }


    public async Task SaveTaskAsync(TaskModel task)
        {
            _logger.LogInformation($"Inside:{nameof(SaveTaskAsync)}");
            try
            {
                task.UpdatedAt = DateTime.UtcNow;

                _dbContext.Tasks.Add(task);
                await _dbContext.SaveChangesAsync();

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
        _logger.LogInformation($"Inside:{nameof(UpdateTaskNameAsync)}");
        try
        {
            var task = await _dbContext.Tasks.FindAsync(taskId);
            if (task == null) return false;

            task.Name = newName;
            task.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            var etag = GenerateEtag(task);
            _cache.Set(task.Id, new CachedTaskEntry { Task = task, ETag = etag }, _cacheOptions);

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
        _logger.LogInformation($"Inside:{nameof(DeleteTaskAsync)}");
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
