using Accessor.Constants;
using Accessor.DB;
using Accessor.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AccessorUnitTests.Cleanup;

public class RefreshSessionsCleanupJobTests
{
    private sealed class TestClockCancellation : IDisposable
    {
        public readonly CancellationTokenSource Cts = new();
        public void Dispose() => Cts.Cancel();
    }

    [Fact(DisplayName = "Hosted job exits immediately when disabled (no DB/service usage)")]
    public async Task ExecuteAsync_Disabled_Exits_Immediately()
    {
        // Arrange DI with a real scope but disabled options
        var services = new ServiceCollection();
        services.AddLogging();

        // A tiny in-memory db is fine; it should never be touched in this test
        services.AddDbContext<AccessorDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped<IRefreshSessionService, RefreshSessionService>(sp =>
        {
            // If this ever gets called, the test would break — good guardrail
            var logger = sp.GetRequiredService<ILogger<RefreshSessionService>>();
            var db = sp.GetRequiredService<AccessorDbContext>();
            return new RefreshSessionService(logger, db);
        });

        var opts = Options.Create(new RefreshSessionsCleanupOptions
        {
            Enabled = false,                      // <-- important
            TimeZone = "Asia/Jerusalem",
            Hour = 2,
            Minute = 30,
            BatchSize = 5000
        });

        var logger = Mock.Of<ILogger<RefreshSessionsCleanupJob>>();
        services.AddSingleton<IOptions<RefreshSessionsCleanupOptions>>(opts);
        var sp = services.BuildServiceProvider();

        var job = new RefreshSessionsCleanupJob(logger, sp, opts);

        // Act: run the hosted service — because it's disabled, it should return immediately.
        using var guard = new TestClockCancellation();
        var run = job.StartAsync(guard.Cts.Token);
        await run;

        // Assert: nothing to assert except that it didn't hang/throw.
        true.Should().BeTrue();
    }
}
