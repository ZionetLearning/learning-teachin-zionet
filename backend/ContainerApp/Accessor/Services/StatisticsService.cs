using Accessor.DB;
using Accessor.Constants;
using Accessor.Models;
using Accessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class StatisticsService : IStatisticsService
{
    private readonly AccessorDbContext _db;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(AccessorDbContext db, ILogger<StatisticsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<StatsSnapshot> ComputeStatsAsync(CancellationToken ct = default)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var from15m = nowUtc.AddMinutes(-StatsWindow.ActiveUsersMinutes);

        var totalThreads = await _db.ChatHistorySnapshots.CountAsync(ct);
        var totalUniqueUsersByThread = await _db.ChatHistorySnapshots
            .Select(c => c.UserId)
            .Distinct()
            .CountAsync(ct);

        var activeUsersLast15m = await _db.ChatHistorySnapshots
            .Where(c => c.UpdatedAt >= from15m)
            .Select(c => c.UserId)
            .Distinct()
            .CountAsync(ct);

        return new StatsSnapshot(
            TotalThreads: totalThreads,
            TotalUniqueUsersByThread: totalUniqueUsersByThread,
            TotalMessages: 0, // future extension
            TotalUniqueUsersByMessage: 0,
            ActiveUsersLast15m: activeUsersLast15m,
            MessagesLast5m: 0,
            MessagesLast15m: 0,
            GeneratedAtUtc: nowUtc
        );
    }
}