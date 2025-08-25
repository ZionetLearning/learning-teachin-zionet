using Dapr.Client;
using FluentAssertions;
using Manager.Constants;
using Manager.Endpoints;
using Manager.Models;
using Manager.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using Xunit;

namespace ManagerUnitTests.Endpoints;

public class ManagerStatsPingEndpointsTests
{
    private static (HttpClient client, Mock<IStatsClient> statsMock) BuildApp()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        var stats = new Mock<IStatsClient>(MockBehavior.Strict);
        builder.Services.AddSingleton<IStatsClient>(stats.Object);
        builder.Services.AddLogging(b => b.AddDebug());

        var app = builder.Build();
        app.MapStatsPing();
        app.RunAsync();

        return (app.GetTestClient(), stats);
    }

    [Fact(DisplayName = "POST /internal/compute-stats/ping => 200 + caches snapshot with TTL")]
    public async Task ComputeStats_Ping_Saves_To_State_With_TTL()
    {
        var (client, stats) = BuildApp();

        var snapshot = new StatsSnapshot(
            TotalThreads: 10,
            TotalUniqueUsersByThread: 7,
            TotalMessages: 55,
            TotalUniqueUsersByMessage: 12,
            ActiveUsersLast15m: 4,
            MessagesLast5m: 3,
            MessagesLast15m: 9,
            GeneratedAtUtc: DateTimeOffset.UtcNow);

        stats.Setup(s => s.GetSnapshotAsync(default)).ReturnsAsync(snapshot);
        stats.Setup(s => s.SaveSnapshotAsync(snapshot, 86400, default)).Returns(Task.CompletedTask);

        var resp = await client.PostAsync("/internal/compute-stats/ping", content: null);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var root = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
        root.GetProperty("ok").GetBoolean().Should().BeTrue();
        root.GetProperty("key").GetString().Should().Be("stats:latest");
        root.GetProperty("ttlSeconds").GetInt32().Should().Be(86400);

        // Default System.Text.Json uses camelCase
        var snap = root.GetProperty("snapshot");
        snap.GetProperty("totalThreads").GetInt64().Should().Be(10);

        stats.VerifyAll();
    }

    [Fact(DisplayName = "POST /internal/compute-stats/ping => 500 Problem on failure")]
    public async Task ComputeStats_Ping_Problem_On_Exception()
    {
        var (client, stats) = BuildApp();

        stats.Setup(s => s.GetSnapshotAsync(default)).ThrowsAsync(new Exception("kaboom"));

        var resp = await client.PostAsync("/internal/compute-stats/ping", content: null);
        resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        stats.VerifyAll();
    }

    [Fact(DisplayName = "GET /internal/stats/latest => 200 with cached snapshot")]
    public async Task Get_Latest_Stats_Returns_Snapshot_When_Present()
    {
        var (client, stats) = BuildApp();

        var snapshot = new StatsSnapshot(
            TotalThreads: 2, TotalUniqueUsersByThread: 2,
            TotalMessages: 3, TotalUniqueUsersByMessage: 3,
            ActiveUsersLast15m: 1, MessagesLast5m: 1, MessagesLast15m: 2,
            GeneratedAtUtc: DateTimeOffset.UtcNow);

        stats.Setup(s => s.TryGetSnapshotAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(snapshot);

        var resp = await client.GetAsync("/internal/stats/latest");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var root = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
        root.GetProperty("ok").GetBoolean().Should().BeTrue();
        root.GetProperty("key").GetString().Should().Be("stats:latest");
        var snap = root.GetProperty("snapshot");
        snap.GetProperty("totalMessages").GetInt64().Should().Be(3);

        stats.VerifyAll();
    }

    [Fact(DisplayName = "GET /internal/stats/latest => 404 when missing")]
    public async Task Get_Latest_Stats_NotFound_When_Missing()
    {
        var (client, stats) = BuildApp();

        stats.Setup(s => s.TryGetSnapshotAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync((StatsSnapshot?)null);

        var resp = await client.GetAsync("/internal/stats/latest");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        stats.VerifyAll();
    }
}
public static class StatsPingEndpoints
{
    public static IEndpointRouteBuilder MapStatsPing(this IEndpointRouteBuilder app)
    {
        // POST: compute & cache for 24h
        app.MapPost("/internal/compute-stats/ping",
            async ([FromServices] ILoggerFactory lf,
                   [FromServices] IStatsClient statsClient) =>
            {
                var log = lf.CreateLogger("StatsCompute");

                try
                {
                    // 1) Invoke Accessor via adapter
                    var snapshot = await statsClient.GetSnapshotAsync(default);

                    // 2) Save to state with TTL via adapter
                    await statsClient.SaveSnapshotAsync(snapshot, StatsKeys.DefaultTtlSeconds, default);

                    log.LogInformation("Saved stats with TTL {TTL}s to key {Key}", StatsKeys.DefaultTtlSeconds, StatsKeys.Latest);

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
            async ([FromServices] ILoggerFactory lf,
                   [FromServices] IStatsClient statsClient,
                   CancellationToken ct) =>
            {
                var log = lf.CreateLogger("StatsRead");

                var snapshot = await statsClient.TryGetSnapshotAsync(ct);

                if (snapshot is null)
                {
                    log.LogWarning("No stats snapshot found for key '{Key}'", StatsKeys.Latest);
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
//helper classes
public interface IStatsClient
{
    Task<StatsSnapshot> GetSnapshotAsync(CancellationToken ct = default);
    Task SaveSnapshotAsync(StatsSnapshot snapshot, int ttlSeconds, CancellationToken ct = default);
    Task<StatsSnapshot?> TryGetSnapshotAsync(CancellationToken ct = default);
}
public sealed class DaprStatsClient : IStatsClient
{
    private readonly DaprClient _dapr;

    private const string StatsKey = "stats:latest";

    public DaprStatsClient(DaprClient dapr)
    {
        _dapr = dapr;
    }

    public async Task<StatsSnapshot> GetSnapshotAsync(CancellationToken ct = default)
    {
        // Calls Accessor through Dapr service invocation
        return await _dapr.InvokeMethodAsync<StatsSnapshot>(
            HttpMethod.Get, AppIds.Accessor, "internal/stats/snapshot", ct);
    }

    public async Task SaveSnapshotAsync(StatsSnapshot snapshot, int ttlSeconds, CancellationToken ct = default)
    {
        var metadata = new Dictionary<string, string> { ["ttlInSeconds"] = ttlSeconds.ToString() };

        await _dapr.SaveStateAsync(
            AppIds.StateStore,
            StatsKey,
            snapshot,
            stateOptions: null,
            metadata: metadata,
            cancellationToken: ct);
    }

    public Task<StatsSnapshot?> TryGetSnapshotAsync(CancellationToken ct = default)
    {
        return _dapr.GetStateAsync<StatsSnapshot?>(
            AppIds.StateStore,
            StatsKey,
            consistencyMode: null,
            metadata: null,
            cancellationToken: ct);
    }
}
