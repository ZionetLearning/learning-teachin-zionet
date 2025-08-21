using Accessor.Models;
using Accessor.Services;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class StatsEndpoints
{
    public static void MapStatsEndpoints(this WebApplication app)
    {
        app.MapGet("/internal/stats/snapshot",
            async ([FromServices] IAccessorService svc,
                   [FromServices] ILogger<AccessorService> logger,
                   CancellationToken ct) =>
            {
                var snap = await svc.ComputeStatsAsync(ct);
                logger.LogInformation("Computed stats {@snap}", snap);
                return Results.Ok(snap);
            })
            .WithName("GetStatsSnapshot")
            .WithTags("Internal")
            .Produces<StatsSnapshot>(StatusCodes.Status200OK);
    }
}
