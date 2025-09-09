using Accessor.Models;

namespace Accessor.Services.Interfaces;

public interface IStatisticsService
{
    Task<StatsSnapshot> ComputeStatsAsync(CancellationToken ct = default);
}