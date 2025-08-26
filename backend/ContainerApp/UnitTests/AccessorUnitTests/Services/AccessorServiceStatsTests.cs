using System;
using System.Threading;
using System.Threading.Tasks;
using Accessor.DB;
using Accessor.Models;
using Accessor.Services;
using Dapr.Client;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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

    private static IConfiguration NewConfig()
    {
        // Provide required keys so the service constructor doesn't explode
        var dict = new System.Collections.Generic.Dictionary<string, string?>
        {
            ["TaskCache:TTLInSeconds"] = "300" // any positive int is fine for tests
            // add more keys here if the service starts requiring them later
            // e.g., ["TaskCache:StateStore"] = "statestore"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    private static AccessorService NewService(AccessorDbContext db)
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Loose);
        var log = Mock.Of<ILogger<AccessorService>>();
        var cfg = NewConfig();
        return new AccessorService(db, log, dapr.Object, cfg);
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

    [Fact(Skip = "Fix after refactoring db")]
    public async Task ComputeStatsAsync_Computes_All_Counters_With_Time_Windows()
    {
        var db = NewDb(Guid.NewGuid().ToString());
        var now = DateTimeOffset.UtcNow;
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();
        // Threads for users u1, u2 (u2 twice)
        //var t1 = new ChatHistorySnapshot { ThreadId = Guid.NewGuid(), UserId = user1, ChatType = "default", CreatedAt = now.AddHours(-2), UpdatedAt = now.AddHours(-1) };
        //var t2 = new ChatHistorySnapshot { ThreadId = Guid.NewGuid(), UserId = user2, ChatType = "default", CreatedAt = now.AddHours(-2), UpdatedAt = now.AddMinutes(-20) };
        //var t3 = new ChatHistorySnapshot { ThreadId = Guid.NewGuid(), UserId = user2, ChatType = "default", CreatedAt = now.AddHours(-3), UpdatedAt = now.AddMinutes(-1) };
        //db.ChatHistorySnapshots.AddRange(t1, t2, t3);

        //// Messages for users: u1 old, u2 within 15m, u3 within 5m
        //db.ChatMessages.AddRange(
        //    new ChatMessage { Id = Guid.NewGuid(), ThreadId = t1.ThreadId, UserId = "u1", Role = MessageRole.User, Content = "old", Timestamp = now.AddMinutes(-40) },
        //    new ChatMessage { Id = Guid.NewGuid(), ThreadId = t2.ThreadId, UserId = "u2", Role = MessageRole.User, Content = "m15", Timestamp = now.AddMinutes(-10) },
        //    new ChatMessage { Id = Guid.NewGuid(), ThreadId = t3.ThreadId, UserId = "u3", Role = MessageRole.Assistant, Content = "m5", Timestamp = now.AddMinutes(-3) }
        //);
        await db.SaveChangesAsync();

        var svc = NewService(db);

        var snap = await svc.ComputeStatsAsync(CancellationToken.None);

        snap.TotalThreads.Should().Be(3);
        snap.TotalUniqueUsersByThread.Should().Be(2);         // u1, u2
        snap.TotalMessages.Should().Be(3);
        snap.TotalUniqueUsersByMessage.Should().Be(3);        // u1, u2, u3
        snap.ActiveUsersLast15m.Should().Be(2);               // u2 and u3 messaged within 15m
        snap.MessagesLast5m.Should().Be(1);                   // one at ~3m
        snap.MessagesLast15m.Should().Be(2);                  // two within 15m
        snap.GeneratedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }
}
