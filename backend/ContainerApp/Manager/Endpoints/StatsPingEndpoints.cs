using Dapr.Client;
using Manager.Constants;
using Manager.Models;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class StatsPingEndpoints
{
    private const string StatsKey = "stats:latest";
    private const int StatsTtlSeconds = 86400;        // 24h
    public static IEndpointRouteBuilder MapStatsPing(this IEndpointRouteBuilder app)
    {
        // POST: compute & cache for 24h
        app.MapPost("/internal/compute-stats/ping",
            async ([FromServices] ILoggerFactory lf,
                   [FromServices] DaprClient dapr,
                   [FromServices] IConfiguration cfg) =>
            {
                var log = lf.CreateLogger("StatsCompute");

                try
                {
                    // 1) Invoke Accessor via Dapr service invocation (no request-abort token)
                    var snapshot = await dapr.InvokeMethodAsync<StatsSnapshot>(
                        HttpMethod.Get, AppIds.Accessor, "internal/stats/snapshot", cancellationToken: default);

                    // 2) Save to state with TTL so Dapr auto-expires it
                    await dapr.SaveStateAsync(
                        storeName: AppIds.StateStore,
                        key: StatsKey,
                        value: snapshot,
                        metadata: new Dictionary<string, string> { ["ttlInSeconds"] = StatsTtlSeconds.ToString() },
                        cancellationToken: default);

                    log.LogInformation("Saved stats to '{StateStore}' key '{Key}' with TTL {TTL}s", AppIds.StateStore, StatsKey, StatsTtlSeconds);

                    return Results.Ok(new { ok = true, key = StatsKey, ttlSeconds = StatsTtlSeconds, snapshot });
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
            async ([FromServices] ILoggerFactory lf,
                   [FromServices] DaprClient dapr,
                   [FromServices] IConfiguration cfg,
                   CancellationToken ct) =>
            {
                var log = lf.CreateLogger("StatsRead");

                var snapshot = await dapr.GetStateAsync<StatsSnapshot>(
                    storeName: AppIds.StateStore, key: StatsKey, cancellationToken: ct);

                if (snapshot is null)
                {
                    log.LogWarning("No stats snapshot found in '{StateStore}' for key '{Key}'", AppIds.StateStore, StatsKey);
                    return Results.NotFound(new { ok = false, message = "No stats snapshot available." });
                }

                return Results.Ok(new
                {
                    ok = true,
                    key = StatsKey,
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
