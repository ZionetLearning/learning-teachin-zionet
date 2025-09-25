using Accessor.Constants;
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

        // Lock to avoid concurrent migration attempts
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT pg_advisory_lock(727274);";
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            // Check if EF thinks migrations are already applied
            var appliedMigrations = await _dbContext.Database.GetAppliedMigrationsAsync();

            if (!appliedMigrations.Any())
            {
                _logger.LogWarning("No EF migrations found in __EFMigrationsHistory. Checking for existing tables...");

                // Check if key table already exists (e.g., Users)
                await using var checkTableCommand = conn.CreateCommand();
                checkTableCommand.CommandText = EfMigrationConstants.CheckIfAnchorTableExistsSql;

                var tableExists = (bool)(await checkTableCommand.ExecuteScalarAsync() ?? false);

                if (tableExists)
                {
                    _logger.LogWarning("Tables exist but no migrations recorded. Seeding fake initial migration...");

                    await using var insertCommand = conn.CreateCommand();
                    insertCommand.CommandText = EfMigrationConstants.InsertInitialMigrationSql;

                    var migrationIdParam = insertCommand.CreateParameter();
                    migrationIdParam.ParameterName = "@migrationId";
                    migrationIdParam.Value = EfMigrationConstants.InitialMigrationId;
                    insertCommand.Parameters.Add(migrationIdParam);

                    var versionParam = insertCommand.CreateParameter();
                    versionParam.ParameterName = "@version";
                    versionParam.Value = EfMigrationConstants.EfCoreVersion;
                    insertCommand.Parameters.Add(versionParam);

                    await insertCommand.ExecuteNonQueryAsync();

                    _logger.LogInformation("Fake migration inserted: {MigrationId}", EfMigrationConstants.InitialMigrationId);
                }
            }
            // Apply real pending migrations
            await _dbContext.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed.");
            throw;
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