using Accessor.Models;
using Microsoft.EntityFrameworkCore;


namespace Accessor.Services;
    public class AccessorService : IAccessorService
    {
        private readonly ILogger<AccessorService> _logger;
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
        _logger.LogInformation($"Inside:{nameof(UpdateTaskNameAsync)}");
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
        _logger.LogInformation($"Inside:{nameof(DeleteTaskAsync)}");
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


    public async Task<TaskModel?> GetTaskByIdAsync(int id)
    {
        _logger.LogInformation($"Inside:{nameof(GetTaskByIdAsync)}");
        try
        {
            var task = await _dbContext.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found.", id);
                return null;
            }

            _logger.LogInformation("Fetched task ID {TaskId}: {TaskName}", task.Id, task.Name);
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
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} saved successfully.", task.Id);
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
    
}
