
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;



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
            //await _dbContext.Database.OpenConnectionAsync();
            //_logger.LogInformation("Connected to PostgreSQL during startup.");
            //await _dbContext.Database.CloseConnectionAsync();
            _logger.LogInformation("Applying EF Core migrations...");
            await _dbContext.Database.MigrateAsync();  // <-- this ensures schema is up-to-date
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









}