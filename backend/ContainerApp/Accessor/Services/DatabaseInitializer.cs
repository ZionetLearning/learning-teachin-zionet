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

        // Lock to avoid concurrent migration runs
        await using (var lockCmd = conn.CreateCommand())
        {
            lockCmd.CommandText = "SELECT pg_advisory_lock(727274);";
            await lockCmd.ExecuteNonQueryAsync();
        }

        try
        {
            // 1. Check which migrations EF thinks have been applied
            var appliedMigrations = await _dbContext.Database.GetAppliedMigrationsAsync();

            // 2. If none applied, possibly this DB was created outside EF
            if (!appliedMigrations.Any())
            {
                _logger.LogWarning("No EF migrations recorded. Checking for existing schema...");

                // 3. Use an anchor table check (one table you expect to exist) to detect pre-existing DB
                await using var checkCmd = conn.CreateCommand();
                checkCmd.CommandText = EfMigrationConstants.CheckIfAnchorTableExistsSql;
                var tableExists = (bool)(await checkCmd.ExecuteScalarAsync() ?? false);

                if (tableExists)
                {
                    _logger.LogWarning("Schema appears to exist without migrations. Inserting initial migration marker.");

                    // 4. Insert the initial migration record into __EFMigrationsHistory
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

                    _logger.LogInformation("Inserted fake initial migration: {MigrationId}", EfMigrationConstants.InitialMigrationId);
                }
            }

            // 5. Now check if there are *any migrations left* to run
            var pending = await _dbContext.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                _logger.LogInformation("Running {Count} pending migrations...", pending.Count());
                await _dbContext.Database.MigrateAsync();
                _logger.LogInformation("Migrations applied.");
            }
            else
            {
                _logger.LogInformation("No pending migrations. Skipping migration step.");
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
