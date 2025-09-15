using Accessor.DB;
using Accessor.Models;
using Accessor.Services;
using Microsoft.EntityFrameworkCore;
using Dapr.Client;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AccessorUnitTests;

public class AccessorServiceTests
{
    // ---------- EF InMemory ----------
    private static AccessorDbContext NewDb(string name)
    {
        var options = new DbContextOptionsBuilder<AccessorDbContext>()
            .UseInMemoryDatabase(name)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .EnableSensitiveDataLogging()
            .Options;

        return new AccessorDbContext(options);
    }

    // ---------- Config / Service ----------
    private static IConfiguration NewConfig(int ttl = 123) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TaskCache:TTLInSeconds"] = ttl.ToString()
            })
            .Build();

    private static TaskService NewTaskService(
        AccessorDbContext db,
        Mock<DaprClient> daprMock,
        int ttl = 123)
    {
        var cfg = NewConfig(ttl);
        var log = Mock.Of<ILogger<TaskService>>();
        return new TaskService(db, log, daprMock.Object, cfg);
    }

    // ---------- UpdateTaskNameAsync ----------
    [Fact]
    public async Task UpdateTaskNameAsync_MissingIfMatch_Returns_PreconditionFailed()
    {
        var db = NewDb(Guid.NewGuid().ToString());
        var dapr = new Mock<DaprClient>(MockBehavior.Loose);
        var svc = NewTaskService(db, dapr);

        var result = await svc.UpdateTaskNameAsync(99, "zzz", null);

        result.Updated.Should().BeFalse();
        result.NotFound.Should().BeFalse();
        result.PreconditionFailed.Should().BeTrue();
        result.NewEtag.Should().BeNull();
    }

    // ---------- DeleteTaskAsync ----------
    [Fact]
    public async Task DeleteTaskAsync_Missing_ReturnsFalse()
    {
        var db = NewDb(Guid.NewGuid().ToString());
        var dapr = new Mock<DaprClient>(MockBehavior.Loose);
        var svc = NewTaskService(db, dapr);

        var ok = await svc.DeleteTaskAsync(404);
        ok.Should().BeFalse();
    }

    //need fix after refactoring chat
    //    // ---------- Threads / Chat ----------
    //    [Fact]
    //    public async Task GetThreadByIdAsync_ReturnsThread()
    //    {
    //        var db = NewDb(Guid.NewGuid().ToString());
    //        var tid = Guid.NewGuid();
    //        db.ChatThreads.Add(new ChatThread { ThreadId = tid, UserId = "u", ChatType = "default", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
    //        await db.SaveChangesAsync();

    //        var svc = NewService(db, new Mock<DaprClient>(MockBehavior.Loose));

    //        var t = await svc.GetThreadByIdAsync(tid);
    //        t.Should().NotBeNull();
    //        t!.ThreadId.Should().Be(tid);
    //    }

    //    [Fact]
    //    public async Task CreateThreadAsync_InsertsRow()
    //    {
    //        var db = NewDb(Guid.NewGuid().ToString());
    //        var svc = NewService(db, new Mock<DaprClient>(MockBehavior.Loose));

    //        var tid = Guid.NewGuid();
    //        var name = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}";
    //        var userId = Guid.NewGuid();

    //        await svc.CreateChatAsync(new ChatHistorySnapshot
    //        {
    //            ThreadId = tid,
    //            UserId = userId,
    //            ChatType = "default",
    //            Name = name,
    //            History = string.Empty,
    //            CreatedAt = DateTimeOffset.UtcNow,
    //            UpdatedAt = DateTimeOffset.UtcNow
    //        });

    //        (await db.ChatThreads.FindAsync(tid)).Should().NotBeNull();
    //    }

    //    [Fact]
    //    public async Task GetThreadsForUserAsync_Filters_Orders_Projects()
    //    {
    //        var db = NewDb(Guid.NewGuid().ToString());
    //        var now = DateTimeOffset.UtcNow;
    //        var userId1 = Guid.NewGuid();
    //        var userId2 = Guid.NewGuid();

    //        db.ChatThreads.AddRange(
    //            new ChatThread { ThreadId = Guid.NewGuid(), UserId = "bob", ChatType = "default", ChatName = "X", CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-1) },
    //            new ChatThread { ThreadId = Guid.NewGuid(), UserId = "alice", ChatType = "default", ChatName = "A", CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-2) },
    //            new ChatThread { ThreadId = Guid.NewGuid(), UserId = "alice", ChatType = "default", ChatName = "B", CreatedAt = now.AddDays(-1), UpdatedAt = now.AddHours(-1) }
    //        );
    //        await db.SaveChangesAsync();

    //        var svc = NewService(db, new Mock<DaprClient>(MockBehavior.Loose));

    //        var list = await svc.GetThreadsForUserAsync(userId1);
    //        list.Should().HaveCount(2);
    //        list.Select(x => x.ChatName).Should().ContainInOrder("B", "A");
    //    }

    //    [Fact]
    //    public async Task AddMessageAsync_AutoCreatesThread_WhenMissing()
    //    {
    //        var db = NewDb(Guid.NewGuid().ToString());
    //        var svc = NewService(db, new Mock<DaprClient>(MockBehavior.Loose));

    //        var tid = Guid.NewGuid();
    //        var ts = DateTimeOffset.UtcNow.AddMinutes(-5);

    //        await svc.AddMessageAsync(new ChatMessage
    //        {
    //            Id = Guid.NewGuid(),
    //            ThreadId = tid,
    //            UserId = "alice",
    //            Role = MessageRole.User,
    //            Content = "hi",
    //            Timestamp = ts
    //        });

    //        var thread = await db.ChatThreads.FindAsync(tid);
    //        thread.Should().NotBeNull();
    //        thread!.CreatedAt.Should().BeCloseTo(ts, TimeSpan.FromSeconds(1));
    //        thread.UpdatedAt.Should().BeCloseTo(ts, TimeSpan.FromSeconds(1));

    //        var msgs = await db.ChatMessages.Where(m => m.ThreadId == tid).ToListAsync();
    //        msgs.Should().ContainSingle(m => m.Content == "hi");
    //    }

    //    [Fact]
    //    public async Task AddMessageAsync_UpdatesThreadTimestamp_WhenExists()
    //    {
    //        var db = NewDb(Guid.NewGuid().ToString());
    //        var tid = Guid.NewGuid();
    //        var old = DateTimeOffset.UtcNow.AddHours(-2);

    //        db.ChatThreads.Add(new ChatThread
    //        {
    //            ThreadId = tid,
    //            UserId = "alice",
    //            ChatType = "default",
    //            CreatedAt = old,
    //            UpdatedAt = old
    //        });
    //        await db.SaveChangesAsync();

    //        var svc = NewService(db, new Mock<DaprClient>(MockBehavior.Loose));

    //        var ts = DateTimeOffset.UtcNow;
    //        await svc.AddMessageAsync(new ChatMessage
    //        {
    //            Id = Guid.NewGuid(),
    //            ThreadId = tid,
    //            UserId = "alice",
    //            Role = MessageRole.Assistant,
    //            Content = "hey",
    //            Timestamp = ts
    //        });

    //        var thread = await db.ChatThreads.FindAsync(tid);
    //        thread!.UpdatedAt.Should().BeAfter(old).And.BeCloseTo(ts, TimeSpan.FromSeconds(2));
    //    }

    //    [Fact]
    //    public async Task GetMessagesByThreadAsync_ReturnsChronological()
    //    {
    //        var db = NewDb(Guid.NewGuid().ToString());
    //        var tid = Guid.NewGuid();
    //        var t1 = DateTimeOffset.UtcNow.AddMinutes(-2);
    //        var t2 = DateTimeOffset.UtcNow.AddMinutes(-1);

    //        db.ChatMessages.AddRange(
    //            new ChatMessage { Id = Guid.NewGuid(), ThreadId = tid, UserId = "u", Role = MessageRole.User, Content = "B", Timestamp = t2 },
    //            new ChatMessage { Id = Guid.NewGuid(), ThreadId = tid, UserId = "u", Role = MessageRole.User, Content = "A", Timestamp = t1 }
    //        );
    //        await db.SaveChangesAsync();

    //        var svc = NewService(db, new Mock<DaprClient>(MockBehavior.Loose));

    //        var list = (await svc.GetMessagesByThreadAsync(tid)).ToList();
    //        list.Select(m => m.Content).Should().ContainInOrder("A", "B");
    //    }
}