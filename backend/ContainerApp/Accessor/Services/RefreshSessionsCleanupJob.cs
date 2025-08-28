using Accessor.Constants;
using Accessor.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Accessor.Services;

public sealed class RefreshSessionsCleanupJob : BackgroundService
{
    private readonly ILogger<RefreshSessionsCleanupJob> _logger;
    private readonly IServiceProvider _sp;
    private readonly RefreshSessionsCleanupOptions _opts;

    // Use a unique advisory lock key for this job
    private const long AdvisoryLockKey = 727275L;

    public RefreshSessionsCleanupJob(
        ILogger<RefreshSessionsCleanupJob> logger,
        IServiceProvider sp,
        IOptions<RefreshSessionsCleanupOptions> opts)
    {
        _logger = logger;
        _sp = sp;
        _opts = opts.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_opts.Enabled)
        {
            _logger.LogInformation("RefreshSessionsCleanupJob disabled via configuration.");
            return;
        }

        var tz = GetTimeZone(_opts.TimeZone);

        while (!stoppingToken.IsCancellationRequested)
        {
            var nowLocal = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
            var todayRun = new DateTimeOffset(
                nowLocal.Year, nowLocal.Month, nowLocal.Day,
                _opts.Hour, _opts.Minute, 0, tz.GetUtcOffset(nowLocal));

            var nextRunLocal = nowLocal <= todayRun ? todayRun : todayRun.AddDays(1);
            var delay = nextRunLocal - nowLocal;
            if (delay < TimeSpan.Zero)
            {
                delay = TimeSpan.Zero;
            }

            _logger.LogInformation("Cleanup job sleeping until {NextRun} ({Delay} from now)", nextRunLocal, delay);
            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            // run once
            await RunOnceAsync(stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AccessorDbContext>();
            var svc = scope.ServiceProvider.GetRequiredService<IRefreshSessionService>();

            await db.Database.OpenConnectionAsync(ct);

            // Try to become the single runner using Postgres advisory lock (non-blocking)
            await using (var cmd = db.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = "SELECT pg_try_advisory_lock(@lockKey);";
                var param = cmd.CreateParameter();
                param.ParameterName = "lockKey";
                param.DbType = System.Data.DbType.Int64;
                param.Value = AdvisoryLockKey;
                cmd.Parameters.Add(param);

                var gotLock = (bool)(await cmd.ExecuteScalarAsync(ct) ?? false);
                if (!gotLock)
                {
                    _logger.LogInformation("Cleanup skipped — another replica holds the lock.");
                    return;
                }
            }

            try
            {
                var batch = Math.Max(100, _opts.BatchSize);
                // Cast to concrete service only if you added the method as concrete;
                // otherwise expose it on the interface.
                var removed = await ((RefreshSessionService)svc).PurgeExpiredOrRevokedAsync(batch, ct);
                _logger.LogInformation("Cleanup finished. Removed {Removed} sessions.", removed);
            }
            finally
            {
                await using var unlock = db.Database.GetDbConnection().CreateCommand();
                unlock.CommandText = "SELECT pg_advisory_unlock(@lockKey);";

                var unlockParam = unlock.CreateParameter();
                unlockParam.ParameterName = "lockKey";
                unlockParam.DbType = System.Data.DbType.Int64;
                unlockParam.Value = AdvisoryLockKey;
                unlock.Parameters.Add(unlockParam);

                await unlock.ExecuteNonQueryAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshSessionsCleanupJob failed");
        }
    }

    private static TimeZoneInfo GetTimeZone(string id)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }
}
