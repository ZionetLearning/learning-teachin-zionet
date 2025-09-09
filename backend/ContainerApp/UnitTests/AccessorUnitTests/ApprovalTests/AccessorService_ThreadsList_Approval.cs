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

    [Fact(Skip = "Fix ApprovalSetup.VerifyJsonClean:  ApprovalSetup needs fixing")]
    public async Task GetThreadsForUser_Snapshot_Projection_And_Order()
    {
        var db = NewDb(Guid.NewGuid().ToString());

        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        // deterministic times/guids for stable snapshot
        var t0 = new DateTimeOffset(2020, 01, 01, 12, 00, 00, TimeSpan.Zero);
        var t1 = t0.AddHours(1);
        var t2 = t0.AddHours(2);

        db.ChatHistorySnapshots.AddRange(
            new ChatHistorySnapshot { ThreadId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), UserId = userId1, History = """{"messages":[]}""", ChatType = "default", Name = "A", CreatedAt = t0, UpdatedAt = t1 },
            new ChatHistorySnapshot { ThreadId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), UserId = userId1, History = """{"messages":[]}""", ChatType = "default", Name = "B", CreatedAt = t0, UpdatedAt = t2 },
            new ChatHistorySnapshot { ThreadId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), UserId = userId2, History = """{"messages":[]}""", ChatType = "default", Name = "X", CreatedAt = t0, UpdatedAt = t2 }
        );
        await db.SaveChangesAsync();

        var dapr = new Mock<DaprClient>(MockBehavior.Loose);
        var log = Mock.Of<ILogger<ChatHistoryService>>();
        var svc = new ChatHistoryService(db, log);

        var list = await svc.GetChatsForUserAsync(userId1);

        // Snapshot (scrub GUIDs & timestamps via your helper)
        ApprovalSetup.VerifyJsonClean(
            JsonConvert.SerializeObject(list, Formatting.Indented),
            additionalInfo: "ThreadsList_alice"
        );
    }
}
