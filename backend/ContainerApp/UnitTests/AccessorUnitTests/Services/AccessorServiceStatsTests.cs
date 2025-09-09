using System;
using System.Threading;
using System.Threading.Tasks;
using Accessor.DB;
using Accessor.Models;
using Accessor.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace AccessorUnitTests;

public class AccessorServiceStatsTests
{
    private static AccessorDbContext NewDb(string name)
    {
        var options = new DbContextOptionsBuilder<AccessorDbContext>()
            .UseInMemoryDatabase(name)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .EnableSensitiveDataLogging()
            .Options;

        return new AccessorDbContext(options);
    }

    private static StatsService NewService(AccessorDbContext db)
    {
        var log = Mock.Of<ILogger<StatsService>>();
        return new StatsService(db, log);
    }

    [Fact]
    public async Task ComputeStatsAsync_EmptyDb_ReturnsZeros()
    {
        var db = NewDb(Guid.NewGuid().ToString());
        var svc = NewService(db);

        var snap = await svc.ComputeStatsAsync(CancellationToken.None);

        snap.TotalThreads.Should().Be(0);
        snap.TotalUniqueUsersByThread.Should().Be(0);
        snap.TotalMessages.Should().Be(0);
        snap.TotalUniqueUsersByMessage.Should().Be(0);
        snap.ActiveUsersLast15m.Should().Be(0);
        snap.MessagesLast5m.Should().Be(0);
        snap.MessagesLast15m.Should().Be(0);
        snap.GeneratedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ComputeStatsAsync_Computes_All_Counters_With_Time_Windows()
    {
        var db = NewDb(Guid.NewGuid().ToString());
        var now = DateTimeOffset.UtcNow;

        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        // Simulate chat threads
        var t1 = new ChatHistorySnapshot
        {
            ThreadId = Guid.NewGuid(),
            UserId = user1,
            ChatType = "default",
            History = "{}", // <-- required
            CreatedAt = now.AddHours(-2),
            UpdatedAt = now.AddHours(-1)
        };

        var t2 = new ChatHistorySnapshot
        {
            ThreadId = Guid.NewGuid(),
            UserId = user2,
            ChatType = "default",
            History = "{}",
            CreatedAt = now.AddHours(-2),
            UpdatedAt = now.AddMinutes(-10) // active within 15m
        };

        await db.ChatHistorySnapshots.AddRangeAsync(t1, t2);
        await db.SaveChangesAsync();

        var svc = NewService(db);

        var snap = await svc.ComputeStatsAsync(CancellationToken.None);

        snap.TotalThreads.Should().Be(2);
        snap.TotalUniqueUsersByThread.Should().Be(2);
        snap.TotalMessages.Should().Be(0);
        snap.TotalUniqueUsersByMessage.Should().Be(0);
        snap.ActiveUsersLast15m.Should().Be(1); // user2 active recently
        snap.MessagesLast5m.Should().Be(0);
        snap.MessagesLast15m.Should().Be(0);
        snap.GeneratedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }
}