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
using Microsoft.EntityFrameworkCore.Storage;
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