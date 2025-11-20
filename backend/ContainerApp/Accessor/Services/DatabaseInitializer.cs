using Accessor.DB;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class DatabaseInitializer
{
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly AccessorDbContext _dbContext;

    public DatabaseInitializer(AccessorDbContext dbContext, ILogger<DatabaseInitializer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Applying EF Core migrations...");

        var conn = _dbContext.Database.GetDbConnection();
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT pg_advisory_lock(727274);";
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            await _dbContext.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed.");
        }
        finally
        {
            await using (var unlock = conn.CreateCommand())
            {
                unlock.CommandText = "SELECT pg_advisory_unlock(727274);";
                await unlock.ExecuteNonQueryAsync();
            }
        }
    }
}