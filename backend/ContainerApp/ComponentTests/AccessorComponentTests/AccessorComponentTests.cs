using System.Net;
using System.Net.Http.Json;
using Accessor.DB;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;   // InMemoryDatabaseRoot
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Accessor.ComponentTests;

// ---------------- Test-only chat service (only the members the endpoints use) ----------------
internal sealed class TestChatService : IAccessorService
{
    private readonly AccessorDbContext _db;
    public TestChatService(AccessorDbContext db) => _db = db;

    public async Task AddMessageAsync(ChatMessage message)
    {
        // server sets it, but normalize anyway
        message.Timestamp = message.Timestamp.ToUniversalTime();

        var thread = await _db.ChatThreads.FindAsync(message.ThreadId);
        if (thread is null)
        {
            thread = new ChatThread
            {
                ThreadId = message.ThreadId,
                UserId = message.UserId,
                ChatType = "default",
                ChatName = "ChatName",
                CreatedAt = message.Timestamp,
                UpdatedAt = message.Timestamp
            };
            _db.ChatThreads.Add(thread);
        }
        else
        {
            thread.UpdatedAt = message.Timestamp;
        }

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync();
    }

    public Task<ChatThread?> GetThreadByIdAsync(Guid threadId)
        => _db.ChatThreads.FindAsync(threadId).AsTask();

    public async Task CreateThreadAsync(ChatThread thread)
    {
        _db.ChatThreads.Add(thread);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesByThreadAsync(Guid threadId) =>
        await _db.ChatMessages.AsNoTracking()
                              .Where(m => m.ThreadId == threadId)
                              .OrderBy(m => m.Timestamp)
                              .ToListAsync();

    public async Task<List<ThreadSummaryDto>> GetThreadsForUserAsync(string userId) =>
        await _db.ChatThreads.AsNoTracking()
                             .Where(t => t.UserId == userId)
                             .OrderByDescending(t => t.UpdatedAt)
                             .Select(t => new ThreadSummaryDto(t.ThreadId, t.ChatName, t.ChatType, t.CreatedAt, t.UpdatedAt))
                             .ToListAsync();

    // Unused by these tests
    public Task InitializeAsync() => Task.CompletedTask;
    public Task<TaskModel?> GetTaskByIdAsync(int id) => throw new NotImplementedException();
    public Task CreateTaskAsync(TaskModel task) => throw new NotImplementedException();
    public Task<bool> UpdateTaskNameAsync(int taskId, string newName) => throw new NotImplementedException();
    public Task<bool> DeleteTaskAsync(int taskId) => throw new NotImplementedException();
}

// ---------------- Minimal host that maps ONLY the three chat endpoints ----------------
public sealed class ChatEndpointsFactory : IDisposable
{
    private readonly WebApplication _app;
    public HttpClient Client { get; }
    public IServiceProvider Services => _app.Services;

    // IMPORTANT: fixed DB name + shared root to ensure the same in-memory store is used
    private static readonly InMemoryDatabaseRoot DbRoot = new();

    public ChatEndpointsFactory()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        builder.WebHost.UseTestServer();

        builder.Services.Configure<JsonOptions>(o =>
        {
            o.SerializerOptions.PropertyNameCaseInsensitive = true;
            // Your models already use JsonStringEnumConverter attributes; this is fine either way.
            o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

        builder.Services.AddDbContext<AccessorDbContext>(opt =>
            opt.UseInMemoryDatabase("ChatComponentTests", DbRoot)); // <— fixed name + shared root

        builder.Services.AddScoped<IAccessorService, TestChatService>();

        _app = builder.Build();

        // map exactly the endpoints you want to test
        _app.MapPost("/threads/message", AccessorEndpoints.StoreMessageAsync);
        _app.MapGet("/threads/{threadId:guid}/messages", AccessorEndpoints.GetChatHistoryAsync).WithName("GetChatHistory");
        _app.MapGet("/threads/{userId}", AccessorEndpoints.GetThreadsForUserAsync);

        using (var scope = _app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AccessorDbContext>();
            db.Database.EnsureCreated();
        }

        _app.StartAsync().GetAwaiter().GetResult();
        Client = _app.GetTestClient();
    }

    public void Dispose()
    {
        Client.Dispose();
        _app.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}

// ---------------- The tests ----------------
public class ChatEndpointsComponentTests : IClassFixture<ChatEndpointsFactory>
{
    private readonly ChatEndpointsFactory _factory;
    private HttpClient Client => _factory.Client;
    public ChatEndpointsComponentTests(ChatEndpointsFactory factory) => _factory = factory;

    [Fact(DisplayName = "POST /threads/message → 201; then GET /threads/{threadId}/messages returns it")]
    public async Task Post_Then_Get_History_Works()
    {
        var threadId = Guid.NewGuid();

        var request = new ChatMessage
        {
            ThreadId = threadId,
            UserId = "alice",
            Role = MessageRole.User,
            Content = "Hello from component test"
        };

        var post = await Client.PostAsJsonAsync("/threads/message", request);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var history = await Client.GetAsync($"/threads/{threadId}/messages");
        history.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await history.Content.ReadFromJsonAsync<List<ChatMessage>>();
        items.Should().NotBeNull();
        items!.Should().ContainSingle();
        items[0].Content.Should().Be("Hello from component test");
        items[0].Role.Should().Be(MessageRole.User);
        items[0].ThreadId.Should().Be(threadId);
    }

    [Fact(DisplayName = "GET /threads/{userId} → returns user thread summaries (after a message)")]
    public async Task Get_Threads_For_User_Works()
    {
        var threadId = Guid.NewGuid();

        // Seed via the API (so it uses the same path as production)
        var msg = new ChatMessage
        {
            ThreadId = threadId,
            UserId = "bob",
            Role = MessageRole.User,
            Content = "First message"
        };
        var post = await Client.PostAsJsonAsync("/threads/message", msg);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var res = await Client.GetAsync($"/threads/{msg.UserId}");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var threads = await res.Content.ReadFromJsonAsync<List<ThreadSummaryDto>>();
        threads.Should().NotBeNull();
        threads!.Should().ContainSingle(t => t.ThreadId == threadId);
    }
}
