// File: AccessorService_CreateTask_CacheWrite_Approval.cs
using Newtonsoft.Json;
using ApprovalTests;
using ApprovalTests.Namers;
using Accessor.DB;
using Accessor.Models;
using Accessor.Services;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccessorUnitTests;

public class AccessorService_CreateTask_CacheWrite_Approval
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

    private static IConfiguration Cfg(int ttl = 321) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["TaskCache:TTLInSeconds"] = ttl.ToString() })
            .Build();

    //[Fact]
    //public async Task CreateTask_Writes_State_With_Ttl_And_Idempotency_Shape_Is_Approved()
    //{
    //    ApprovalSetup.Init();

    //    var db = NewDb(Guid.NewGuid().ToString());
    //    var ttl = 321;
    //    var log = Mock.Of<ILogger<AccessorService>>();

    //    string? capturedKey = null;
    //    object? capturedVal = null;
    //    IReadOnlyDictionary<string, string>? capturedMeta = null;

    //    var dapr = new Mock<DaprClient>(MockBehavior.Strict);
    //    dapr.Setup(d => d.SaveStateAsync(
    //            It.IsAny<string>(),
    //            It.IsAny<string>(),
    //            It.IsAny<object>(),
    //            It.IsAny<IReadOnlyDictionary<string, string>>(),
    //            It.IsAny<CancellationToken>()))
    //        .Callback<string, string, object, IReadOnlyDictionary<string, string>, CancellationToken>((store, key, value, meta, _) =>
    //        {
    //            capturedKey = key;
    //            capturedVal = value;
    //            capturedMeta = meta;
    //        })
    //        .Returns(Task.CompletedTask);

    //    var sut = new AccessorService(db, log, dapr.Object, Cfg(ttl));
    //    var task = new TaskModel { Id = 42, Name = "demo" };

    //    await sut.CreateTaskAsync(task);

    //    var gate = db.Idempotency.SingleOrDefault(i => i.IdempotencyKey == "42");

    //    var snapshot = new
    //    {
    //        cacheKey = capturedKey,
    //        payload = capturedVal,     // JSON will be produced by VerifyJson
    //        metadata = capturedMeta,    // expects { "ttlInSeconds": "321" }
    //        idempotency = gate is null ? null : new
    //        {
    //            gate.IdempotencyKey,
    //            gate.Status,
    //            CreatedAtUtc = "2020-01-01T00:00:00Z", // scrubbed by default scrubber
    //            ExpiresAtUtc = gate.ExpiresAtUtc?.ToString("o") ?? null
    //        }
    //    };

    //    NamerFactory.AdditionalInformation = "CreateTask_StateAndIdempotency";
    //    Approvals.VerifyJson(JsonConvert.SerializeObject(snapshot, Formatting.Indented));
    //    NamerFactory.AdditionalInformation = null;

    //    dapr.VerifyAll();
    //}
}
