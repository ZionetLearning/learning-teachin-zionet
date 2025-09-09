using System.Net;
using System.Net.Http.Json;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Services;
using Accessor.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AccessorUnitTests.Endpoints;

public class AccessorStatsEndpointsTests
{
    private static HttpClient BuildClientReturning(StatsSnapshot snapshot, out Mock<IStatsService> svcMock)
    {
        svcMock = new Mock<IStatsService>(MockBehavior.Strict);
        svcMock.Setup(s => s.ComputeStatsAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(snapshot);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddSingleton(svcMock.Object);
        builder.Services.AddLogging(x => x.AddDebug());

        var app = builder.Build();
        app.MapStatsEndpoints(); // extension method now depends on IStatsService

        app.RunAsync(); // start test server
        return app.GetTestClient();
    }

    [Fact(DisplayName = "GET /internal-accessor/stats/snapshot => 200 + body")]
    public async Task Snapshot_Returns_Ok_With_Snapshot()
    {
        var expected = new StatsSnapshot(
            TotalThreads: 5,
            TotalUniqueUsersByThread: 3,
            TotalMessages: 12,
            TotalUniqueUsersByMessage: 4,
            ActiveUsersLast15m: 2,
            MessagesLast5m: 1,
            MessagesLast15m: 3,
            GeneratedAtUtc: DateTimeOffset.UtcNow);

        var client = BuildClientReturning(expected, out var svc);

        var resp = await client.GetAsync("/internal-accessor/stats/snapshot");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var got = await resp.Content.ReadFromJsonAsync<StatsSnapshot>();
        got.Should().NotBeNull();
        got!.TotalThreads.Should().Be(expected.TotalThreads);
        got.TotalUniqueUsersByThread.Should().Be(expected.TotalUniqueUsersByThread);
        got.TotalMessages.Should().Be(expected.TotalMessages);
        got.TotalUniqueUsersByMessage.Should().Be(expected.TotalUniqueUsersByMessage);
        got.ActiveUsersLast15m.Should().Be(expected.ActiveUsersLast15m);
        got.MessagesLast5m.Should().Be(expected.MessagesLast5m);
        got.MessagesLast15m.Should().Be(expected.MessagesLast15m);

        svc.VerifyAll();
    }
}