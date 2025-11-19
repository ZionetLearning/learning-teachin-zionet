using Accessor.DB;
using Accessor.Models;
using Accessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class ChatService : IChatService
{
    private readonly AccessorDbContext _db;
    private readonly ILogger<ChatService> _logger;

    public ChatService(AccessorDbContext db, ILogger<ChatService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task CreateChatAsync(ChatHistorySnapshot chat)
    {
        chat.History ??= "null";
        await _db.ChatHistorySnapshots.AddAsync(chat);
        await _db.SaveChangesAsync();
    }

    public Task<List<ChatSummaryDto>> GetChatsForUserAsync(Guid userId) =>
        _db.ChatHistorySnapshots
           .Where(c => c.UserId == userId)
           .OrderByDescending(c => c.UpdatedAt)
           .Select(c => new ChatSummaryDto(
               c.ThreadId,
               c.Name ?? string.Empty,
               c.ChatType ?? string.Empty,
               c.CreatedAt,
               c.UpdatedAt))
           .ToListAsync();

    public Task<ChatHistorySnapshot?> GetHistorySnapshotAsync(Guid threadId) =>
        _db.ChatHistorySnapshots.FirstOrDefaultAsync(c => c.ThreadId == threadId);

    public async Task UpsertHistorySnapshotAsync(ChatHistorySnapshot snapshot)
    {
        var existing = await _db.ChatHistorySnapshots.FirstOrDefaultAsync(c => c.ThreadId == snapshot.ThreadId);
        var now = DateTimeOffset.UtcNow;

        if (existing is null)
        {
            snapshot.CreatedAt = now;
            snapshot.UpdatedAt = now;
            snapshot.History ??= "null";
            await _db.ChatHistorySnapshots.AddAsync(snapshot);
        }
        else
        {
            existing.UserId = snapshot.UserId;
            existing.ChatType = snapshot.ChatType;
            if (!string.IsNullOrWhiteSpace(snapshot.Name))
            {
                existing.Name = snapshot.Name;
            }

            existing.History = string.IsNullOrWhiteSpace(snapshot.History) ? "null" : snapshot.History;
            existing.UpdatedAt = now;
        }

        await _db.SaveChangesAsync();
    }
}
