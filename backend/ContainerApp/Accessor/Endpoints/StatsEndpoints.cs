using Accessor.Models;
using Accessor.Services;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class StatsEndpoints
{
    public static void MapStatsEndpoints(this WebApplication app)
    {
        app.MapGet("/internal-accessor/stats/snapshot",
            async ([FromServices] IStatisticsService statsService,
                   [FromServices] ILogger<StatisticsService> logger, CancellationToken ct) =>
            {
                var snap = await statsService.ComputeStatsAsync(ct);
                if (snap is null)
                {
                    logger.LogWarning("ComputeStatsAsync returned null snapshot");
                    return Results.NotFound(new { ok = false, message = "No stats snapshot available." });
                }

                logger.LogInformation("Computed stats {@snap}", snap);
                return Results.Ok(snap);
            })
            .WithName("GetStatsSnapshot")
            .WithTags("Internal")
            .Produces<StatsSnapshot>(StatusCodes.Status200OK);
    }
}