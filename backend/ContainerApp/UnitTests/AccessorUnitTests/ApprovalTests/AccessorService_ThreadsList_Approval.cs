using Newtonsoft.Json;
using Accessor.DB;
using Accessor.Models;
using Accessor.Services;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccessorUnitTests.ApprovalTests;

public class AccessorService_ThreadsList_Approval
{
    private static AccessorDbContext NewDb(string name)
    {
        var opts = new DbContextOptionsBuilder<AccessorDbContext>()
            .UseInMemoryDatabase(name)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .EnableSensitiveDataLogging()
            .Options;
        return new AccessorDbContext(opts);
    }

    private static IConfiguration Cfg() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["TaskCache:TTLInSeconds"] = "123" }).Build();

    [Fact]
    public async Task GetThreadsForUser_Snapshot_Projection_And_Order()
    {
        var db = NewDb(Guid.NewGuid().ToString());

        // deterministic times/guids for stable snapshot
        var t0 = new DateTimeOffset(2020, 01, 01, 12, 00, 00, TimeSpan.Zero);
        var t1 = t0.AddHours(1);
        var t2 = t0.AddHours(2);

        db.ChatThreads.AddRange(
            new ChatThread { ThreadId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), UserId = "alice", ChatType = "default", ChatName = "A", CreatedAt = t0, UpdatedAt = t1 },
            new ChatThread { ThreadId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), UserId = "alice", ChatType = "default", ChatName = "B", CreatedAt = t0, UpdatedAt = t2 },
            new ChatThread { ThreadId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), UserId = "bob", ChatType = "default", ChatName = "X", CreatedAt = t0, UpdatedAt = t2 }
        );
        await db.SaveChangesAsync();

        var dapr = new Mock<DaprClient>(MockBehavior.Loose);
        var log = Mock.Of<ILogger<AccessorService>>();
        var svc = new AccessorService(db, log, dapr.Object, Cfg());

        var list = await svc.GetThreadsForUserAsync("alice");

        // Snapshot (scrub GUIDs & timestamps via your helper)
        ApprovalSetup.VerifyJsonClean(
            JsonConvert.SerializeObject(list, Formatting.Indented),
            additionalInfo: "ThreadsList_alice"
        );
    }
}
