using Dapr.Client;
using Manager.Constants;
using Manager.Models;
using Manager.Services.Clients;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class StatsPingEndpoints
{
    public static IEndpointRouteBuilder MapStatsPing(this IEndpointRouteBuilder app)
    {
        // POST: compute & cache for 24h
        app.MapPost("/internal/compute-stats/ping",
            async ([FromServices] ILogger log,
                   [FromServices] IAccessorClient accessorClient,
                   [FromServices] DaprClient dapr,
                   CancellationToken ct) =>
            {
                try
                {
                    // 1) Invoke Accessor via Dapr service invocation (no request-abort token)
                    var snapshot = await accessorClient.GetStatsSnapshotAsync(ct);
                    if (snapshot is null)
                    {
                        log.LogWarning("Received null stats snapshot from Accessor");
                        return Results.Problem("No stats snapshot returned from Accessor");
                    }

                    // 2) Save to state with TTL so Dapr auto-expires it
                    await dapr.SaveStateAsync(
                        storeName: AppIds.StateStore,
                        key: StatsKeys.Latest,
                        value: snapshot,
                        metadata: new Dictionary<string, string> { ["ttlInSeconds"] = StatsKeys.DefaultTtlSeconds.ToString() },
                        cancellationToken: ct);

                    log.LogInformation("Saved stats to '{StateStore}' key '{Key}' with TTL {TTL}s", AppIds.StateStore, StatsKeys.Latest, StatsKeys.DefaultTtlSeconds);

                    return Results.Ok(new { ok = true, key = StatsKeys.Latest, ttlSeconds = StatsKeys.DefaultTtlSeconds, snapshot });
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed to compute & cache stats");
                    return Results.Problem("Failed to compute & cache stats");
                }
            })
            .WithName("ComputeStats")
            .WithTags("Internal")
            .Produces(StatusCodes.Status200OK);

        // GET: latest cached stats (404 if expired / not set)
        app.MapGet("/internal/stats/latest",
            async ([FromServices] ILogger log,
                   [FromServices] DaprClient dapr,
                   CancellationToken ct) =>
            {
                var snapshot = await dapr.GetStateAsync<StatsSnapshot>(
                    storeName: AppIds.StateStore, key: StatsKeys.Latest, cancellationToken: ct);

                if (snapshot is null)
                {
                    log.LogWarning("No stats snapshot found in '{StateStore}' for key '{Key}'", AppIds.StateStore, StatsKeys.Latest);
                    return Results.NotFound(new { ok = false, message = "No stats snapshot available." });
                }

                return Results.Ok(new
                {
                    ok = true,
                    key = StatsKeys.Latest,
                    retrievedAtUtc = DateTimeOffset.UtcNow,
                    snapshot
                });
            })
            .WithName("GetLatestStats")
            .WithTags("Internal")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
