using Accessor.Models;

namespace Accessor.Services.Interfaces;

public interface IStatsService
{
    Task<StatsSnapshot> ComputeStatsAsync(CancellationToken ct = default);
}