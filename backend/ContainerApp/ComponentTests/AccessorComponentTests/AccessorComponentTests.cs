using Accessor.DB;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Services;
using Dapr;
using Dapr.Client;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;          // <- UseInMemoryDatabase lives here
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Accessor.ComponentTests;

public sealed class AccessorComponentFactory : IDisposable
{
    private readonly WebApplication _app;
    public HttpClient Client { get; }
    public IServiceProvider Services => _app.Services;

    public AccessorComponentFactory()
    {
        // Minimal config expected by AccessorService
        var cfg = new Dictionary<string, string?>
        {
            ["TaskCache:TTLInSeconds"] = "300"
        };

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        // Use in-memory test server with the Minimal API pipeline
        builder.WebHost.UseTestServer();                             // official pattern for testing Minimal APIs :contentReference[oaicite:2]{index=2}
        builder.Configuration.AddInMemoryCollection(cfg);

        // Logging (quiet by default)
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        // JSON setup (optional)
        builder.Services.Configure<JsonOptions>(o =>
        {
            o.SerializerOptions.PropertyNameCaseInsensitive = true;
            o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

        // ---------- DbContext (Option A: EF InMemory) ----------
        builder.Services.AddDbContext<AccessorDbContext>(opt =>
            opt.UseInMemoryDatabase($"AccessorTests-{Guid.NewGuid()}"));

        // ---------- Dapr client mock with correct signatures ----------
        var dapr = new Mock<DaprClient>(MockBehavior.Loose);

        dapr.Setup(x => x.GetStateEntryAsync<TaskModel>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ConsistencyMode?>(),
                It.IsAny<IReadOnlyDictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string store, string key, ConsistencyMode? _, IReadOnlyDictionary<string, string> __, CancellationToken ___) =>
                new StateEntry<TaskModel>(dapr.Object, store, key, value: default!, etag: string.Empty));

        dapr.Setup(x => x.SaveStateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<StateOptions?>(),
                It.IsAny<IReadOnlyDictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        builder.Services.AddSingleton<DaprClient>(dapr.Object);

        // SUT services
        builder.Services.AddScoped<IAccessorService, AccessorService>();

        // Build app and map the REAL endpoints from your code
        _app = builder.Build();
        _app.MapAccessorEndpoints();                                     // real mapping from AccessorEndpoints.cs

        // Start the in-memory server and create client
        _app.StartAsync().GetAwaiter().GetResult();
        Client = _app.GetTestClient();                                   // provided by UseTestServer()
    }

    public void Dispose()
    {
        Client.Dispose();
        _app.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    // ----------- [ALT: SQLite in-memory replacement] -----------
    // If InMemory still won’t compile/run in your environment, replace the single
    // UseInMemoryDatabase line above with this block and add the two Sqlite packages.
    //
    // builder.Services.AddDbContext<AccessorDbContext>(opt =>
    // {
    //     var conn = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
    //     conn.Open();
    //     opt.UseSqlite(conn);
    // });
    // -----------------------------------------------------------
}

public class AccessorComponentTests : IClassFixture<AccessorComponentFactory>
{
    private readonly AccessorComponentFactory _factory;
    private HttpClient Client => _factory.Client;

    public AccessorComponentTests(AccessorComponentFactory factory) => _factory = factory;

    [Fact(DisplayName = "POST /task then GET /task/{id} -> round-trip TaskModel")]
    public async Task Create_And_Get_Task_Works()
    {
        var model = new TaskModel { Id = 1001, Name = "initial" };

        var create = await Client.PostAsJsonAsync("/task", model);
        create.StatusCode.Should().Be(HttpStatusCode.OK);

        var get = await Client.GetAsync($"/task/{model.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var roundTrip = await get.Content.ReadFromJsonAsync<TaskModel>();
        roundTrip.Should().NotBeNull();
        roundTrip!.Id.Should().Be(1001);
        roundTrip.Name.Should().Be("initial");
    }

    [Fact(DisplayName = "POST /threads/message -> 201 Created and persisted; then GET history returns it")]
    public async Task Chat_Message_Flow_Works()
    {
        var threadId = Guid.NewGuid();
        var request = new ChatMessage
        {
            ThreadId = threadId,
            UserId = "alice",
            Role = MessageRole.User,
            Content = "Hello from component test"
        };

        var response = await Client.PostAsJsonAsync("/threads/message", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ChatMessage>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBe(Guid.Empty);
        created.Timestamp.Offset.Should().Be(TimeSpan.Zero); // UTC

        var history = await Client.GetAsync($"/threads/{threadId}/messages");
        history.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await history.Content.ReadFromJsonAsync<List<ChatMessage>>();
        items!.Should().ContainSingle();
        items[0].Content.Should().Be("Hello from component test");
        items[0].Role.Should().Be(MessageRole.User);
    }

    [Fact(DisplayName = "AccessorQueueHandler(UpdateTask) updates DB (component test without HTTP)")]
    public async Task Queue_Handler_Updates_Task_Name()
    {
        // Seed DB
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AccessorDbContext>();
            db.Tasks.Add(new TaskModel { Id = 2002, Name = "before" });
            await db.SaveChangesAsync();
        }

        // Invoke handler directly
        using (var scope = _factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IAccessorService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Accessor.Endpoints.AccessorQueueHandler>>();
            var handler = new Accessor.Endpoints.AccessorQueueHandler(svc, logger);

            var payload = new TaskModel { Id = 2002, Name = "after" };
            var msg = new Message
            {
                ActionName = MessageAction.UpdateTask,
                Payload = JsonSerializer.SerializeToElement(payload)
            };

            await handler.HandleAsync(msg, renewLock: () => Task.CompletedTask, CancellationToken.None);
        }

        // Assert DB updated
        using var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<AccessorDbContext>();
        (await db2.Tasks.FindAsync(2002))!.Name.Should().Be("after");
    }
}
