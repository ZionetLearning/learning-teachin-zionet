using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class StatsPingEndpoints
{
    private const string DefaultAccessorAppId = "accessor";   // Dapr app-id of Accessor
    private const string DefaultStateStore = "statestore"; // Dapr state store component name
    private const string StatsKey = "stats:latest";
    private const int StatsTtlSeconds = 600;          // 10 minutes

    // Local DTO (shape must match Accessor.Models.StatsSnapshot)
    public record StatsSnapshot(long TotalUsers, long TotalThreads, long TotalMessages, DateTimeOffset GeneratedAtUtc);

    public static IEndpointRouteBuilder MapStatsPing(this IEndpointRouteBuilder app)
    {
        // POST: trigger compute in Accessor and cache in state (what you already had)
        app.MapPost("/internal/compute-stats/ping",
            async ([FromServices] ILoggerFactory lf,
                   [FromServices] DaprClient dapr,
                   [FromServices] IConfiguration cfg,
                   CancellationToken ct) =>
            {
                var log = lf.CreateLogger("StatsCompute");
                var accessorAppId = cfg["Services:Accessor:AppId"] ?? DefaultAccessorAppId;
                var stateStore = cfg["Services:StateStore:Name"] ?? DefaultStateStore;

                try
                {
                    // 1) Fetch snapshot from Accessor via Dapr service invocation
                    var snapshot = await dapr.InvokeMethodAsync<StatsSnapshot>(
                        HttpMethod.Get, accessorAppId, "internal/stats/snapshot", ct);

                    // 2) Save to Dapr state with TTL
                    await dapr.SaveStateAsync(
                        storeName: stateStore,
                        key: StatsKey,
                        value: snapshot,
                        metadata: new Dictionary<string, string> { ["ttlInSeconds"] = StatsTtlSeconds.ToString() },
                        cancellationToken: ct);

                    log.LogInformation("Stats snapshot saved to '{StateStore}' key '{Key}' (TTL {TTL}s): {@Snapshot}",
                        stateStore, StatsKey, StatsTtlSeconds, snapshot);

                    return Results.Ok(new { ok = true, savedKey = StatsKey, ttlSeconds = StatsTtlSeconds, snapshot });
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

        // NEW: GET latest cached stats from Dapr state
        app.MapGet("/internal/stats/latest",
            async ([FromServices] ILoggerFactory lf,
                   [FromServices] DaprClient dapr,
                   [FromServices] IConfiguration cfg,
                   CancellationToken ct) =>
            {
                var log = lf.CreateLogger("StatsRead");
                var stateStore = cfg["Services:StateStore:Name"] ?? DefaultStateStore;

                var snapshot = await dapr.GetStateAsync<StatsSnapshot>(
                    storeName: stateStore,
                    key: StatsKey,
                    cancellationToken: ct);

                if (snapshot is null)
                {
                    log.LogWarning("No stats snapshot found in '{StateStore}' under key '{Key}'", stateStore, StatsKey);
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
