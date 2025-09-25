using Accessor.Constants;
using Accessor.DB;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

        await using var conn = (NpgsqlConnection)_dbContext.Database.GetDbConnection();
        await conn.OpenAsync();

        // Lock to avoid concurrent migration attempts
        await using (var lockCmd = conn.CreateCommand())
        {
            lockCmd.CommandText = "SELECT pg_advisory_lock(727274);";
            await lockCmd.ExecuteNonQueryAsync();
        }

        try
        {
            var appliedMigrations = await _dbContext.Database.GetAppliedMigrationsAsync();

            if (!appliedMigrations.Any())
            {
                _logger.LogWarning("No EF migrations found in __EFMigrationsHistory. Checking for existing tables...");

                // Check if anchor table exists
                await using var checkCmd = conn.CreateCommand();
                checkCmd.CommandText = EfMigrationConstants.CheckIfAnchorTableExistsSql;

                var tableExists = (bool)(await checkCmd.ExecuteScalarAsync() ?? false);

                if (tableExists)
                {
                    _logger.LogWarning("Tables exist but no migrations recorded. Seeding fake initial migration...");

                    await using var insertCmd = conn.CreateCommand();
                    insertCmd.CommandText = EfMigrationConstants.InsertInitialMigrationSql;

                    var migrationIdParam = insertCmd.CreateParameter();
                    migrationIdParam.ParameterName = "@migrationId";
                    migrationIdParam.Value = EfMigrationConstants.InitialMigrationId;
                    insertCmd.Parameters.Add(migrationIdParam);

                    var versionParam = insertCmd.CreateParameter();
                    versionParam.ParameterName = "@version";
                    versionParam.Value = EfMigrationConstants.EfCoreVersion;
                    insertCmd.Parameters.Add(versionParam);

                    await insertCmd.ExecuteNonQueryAsync();

                    _logger.LogInformation("Fake migration inserted: {MigrationId}", EfMigrationConstants.InitialMigrationId);
                }
            }

            // Let EF manage the rest of the connection work
            //await _dbContext.Database.MigrateAsync();
            //_logger.LogInformation("Database migration completed.");

            // FIX: only run MigrateAsync if there are real pending migrations
            var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                await _dbContext.Database.MigrateAsync();
                _logger.LogInformation("Database migration completed.");
            }
            else
            {
                _logger.LogInformation("No pending migrations to apply.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed.");
            throw;
        }
        finally
        {
            await using var unlockCmd = conn.CreateCommand();
            unlockCmd.CommandText = "SELECT pg_advisory_unlock(727274);";
            await unlockCmd.ExecuteNonQueryAsync();

            await conn.CloseAsync();
        }
    }
}
