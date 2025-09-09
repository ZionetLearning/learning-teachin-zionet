using Accessor.Constants;
using Accessor.DB;
using Accessor.Models;
using Microsoft.EntityFrameworkCore;
using Accessor.Models.Users;
using Accessor.Models.Auth;
using Accessor.Exceptions;

namespace Accessor.Services;
public class AccessorService : IAccessorService
{
    private readonly ILogger<AccessorService> _logger;
    private readonly AccessorDbContext _dbContext;

    public AccessorService(AccessorDbContext dbContext,
        ILogger<AccessorService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task InitializeAsync()
    {
        _logger.LogInformation("Applying EF Core migrations...");

        var conn = _dbContext.Database.GetDbConnection();
        await conn.OpenAsync();

        // Take a cluster-wide advisory lock (pick any constant bigint key)
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT pg_advisory_lock(727274);";
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            await _dbContext.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize PostgreSQL during startup.");
            throw;
        }

        finally
        {
            await using var unlock = conn.CreateCommand();
            unlock.CommandText = "SELECT pg_advisory_unlock(727274);";
            await unlock.ExecuteNonQueryAsync();
        }
    }

    public async Task<TaskModel?> GetTaskByIdAsync(int id)
    {
        using var scope = _logger.BeginScope("TaskId: {TaskId}", id);
        _logger.LogInformation("Inside:{Method}", nameof(GetTaskByIdAsync));

        try
        {
            return await _dbContext.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task");
            throw;
        }
    }

    public async Task<StatsSnapshot> ComputeStatsAsync(CancellationToken ct = default)
    {
        try
        {
            var nowUtc = DateTimeOffset.UtcNow;
            var from15m = nowUtc.AddMinutes(-StatsWindow.ActiveUsersMinutes);
            var from5m = nowUtc.AddMinutes(-StatsWindow.MessagesLast5m);

            var totalThreads = await _dbContext.ChatHistorySnapshots
                .AsNoTracking()
                .LongCountAsync(ct);

            var totalUniqueUsersByThread = await _dbContext.ChatHistorySnapshots
                .AsNoTracking()
                .Select(t => t.UserId)
                .Distinct()
                .LongCountAsync(ct);

            //var totalMessages = await _dbContext.ChatMessages
            //    .AsNoTracking()
            //    .LongCountAsync(ct);

            //var totalUniqueUsersByMessage = await _dbContext.ChatMessages
            //    .AsNoTracking()
            //    .Select(m => m.UserId)
            //    .Distinct()
            //    .LongCountAsync(ct);

            var activeUsersLast15m = await _dbContext.ChatHistorySnapshots
                .AsNoTracking()
                .Where(m => m.UpdatedAt >= from15m)
                .Select(m => m.UserId)
                .Distinct()
                .LongCountAsync(ct);

            //var messagesLast5m = await _dbContext.ChatMessages
            //    .AsNoTracking()
            //    .Where(m => m.Timestamp >= from5m)
            //    .LongCountAsync(ct);

            //var messagesLast15m = await _dbContext.ChatMessages
            //    .AsNoTracking()
            //    .Where(m => m.Timestamp >= from15m)
            //    .LongCountAsync(ct);

            return new StatsSnapshot(
                TotalThreads: totalThreads,
                TotalUniqueUsersByThread: totalUniqueUsersByThread,
                TotalMessages: 0,
                TotalUniqueUsersByMessage: 0,
                ActiveUsersLast15m: activeUsersLast15m,
                MessagesLast5m: 0,
                MessagesLast15m: 0,
                GeneratedAtUtc: nowUtc
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute stats");
            throw;
        }
    }

    public async Task CreateTaskAsync(TaskModel task)
    {
        using var scope = _logger.BeginScope("TaskId: {TaskId}", task.Id);
        _logger.LogInformation("Inside:{Method}", nameof(CreateTaskAsync));

        try
        {
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Task {TaskId} persisted", task.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Duplicate Task insert for {TaskId}", task.Id);

            var dbTask = await _dbContext.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == task.Id);
            if (dbTask is null)
            {
                throw;
            }

            if (dbTask.Id != task.Id || dbTask.Name != task.Name)
            {
                throw new ConflictException($"Task {task.Id} already exists with different payload.");
            }

            _logger.LogInformation("Idempotent retry for Task {TaskId}, no-op", task.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating Task {TaskId}", task.Id);
            throw new RetryableException("Unexpected infrastructure error while creating task.", ex);
        }
    }

    private bool TasksAreEqual(TaskModel a, TaskModel b)
    {
        // Minimal equality check for idempotency
        return a.Id == b.Id && a.Name == b.Name;
    }

    public async Task<bool> DeleteTaskAsync(int taskId)
    {
        using var scope = _logger.BeginScope("TaskId: {TaskId}", taskId);
        _logger.LogInformation("Inside:{Method}", nameof(DeleteTaskAsync));

        try
        {
            var task = await _dbContext.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return false;
            }

            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete task.");
            return false;
        }
    }

    private static string GetTaskCacheKey(int taskId) => $"task:{taskId}";

    public async Task CreateChatAsync(ChatHistorySnapshot chat)
    {
        _dbContext.ChatHistorySnapshots.Add(chat);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<ChatSummaryDto>> GetChatsForUserAsync(Guid userId)
    {
        return await _dbContext.ChatHistorySnapshots
            .AsNoTracking() // read-only path
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.UpdatedAt)
            .Select(t => new ChatSummaryDto(t.ThreadId, t.Name, t.ChatType, t.CreatedAt, t.UpdatedAt))
            .ToListAsync();
    }

    public async Task<AuthenticatedUser?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _dbContext.Users
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return null;
        }

        var response = new AuthenticatedUser
        {
            UserId = user.UserId,
            Role = user.Role
        };

        return response;
    }

    public async Task<UserData?> GetUserAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        return user == null
            ? null
            : new UserData
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                PreferredLanguageCode = user.PreferredLanguageCode,
                HebrewLevelValue = user.HebrewLevelValue
            };
    }

    public async Task<bool> CreateUserAsync(UserModel newUser)
    {
        var exists = await _dbContext.Users.AnyAsync(u => u.Email == newUser.Email);
        if (exists)
        {
            return false;
        }

        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserAsync(UpdateUserModel updateUser, Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (updateUser.FirstName is not null)
        {
            user.FirstName = updateUser.FirstName;
        }

        if (updateUser.LastName is not null)
        {
            user.LastName = updateUser.LastName;
        }

        if (updateUser.Email is not null)
        {
            user.Email = updateUser.Email;
        }

        if (updateUser.PreferredLanguageCode is not null)
        {
            user.PreferredLanguageCode = updateUser.PreferredLanguageCode.Value;
        }

        if (updateUser.HebrewLevelValue is not null)
        {
            user.HebrewLevelValue = updateUser.HebrewLevelValue;
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<ChatHistorySnapshot?> GetHistorySnapshotAsync(Guid threadId)
    {
        return await _dbContext.ChatHistorySnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ThreadId == threadId);
    }

    public async Task UpsertHistorySnapshotAsync(ChatHistorySnapshot snapshot)
    {
        var existing = await _dbContext.ChatHistorySnapshots.FirstOrDefaultAsync(x => x.ThreadId == snapshot.ThreadId);
        var now = DateTimeOffset.UtcNow;

        if (existing is null)
        {
            snapshot.CreatedAt = now;
            snapshot.UpdatedAt = now;
            _dbContext.ChatHistorySnapshots.Add(snapshot);
        }
        else
        {
            existing.UserId = snapshot.UserId;
            existing.ChatType = snapshot.ChatType;
            existing.Name = snapshot.Name;
            existing.History = snapshot.History;
            existing.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserData>> GetAllUsersAsync()
    {
        _logger.LogInformation("Fetching all users from the database...");

        try
        {
            var users = await _dbContext.Users
                .AsNoTracking()
                .Select(u => new UserData
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role,
                    PreferredLanguageCode = u.PreferredLanguageCode,
                    HebrewLevelValue = u.HebrewLevelValue
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} users", users.Count);
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve users");
            throw;
        }
    }
    private static string NormalizeIfMatch(string? ifMatch)
    {
        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return string.Empty;
        }

        var s = ifMatch.Trim();
        if (s.StartsWith("W/"))
        {
            s = s[2..].Trim();
        }

        return s.Trim('"');
    }

    public async Task<string?> GetDbEtagAsync(int id, CancellationToken ct = default)
    {
        var xmin = await _dbContext.Tasks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => (uint?)EF.Property<uint>(t, "xmin"))
            .SingleOrDefaultAsync(ct);

        return xmin?.ToString();
    }

    public async Task<(TaskModel Task, string ETag)?> GetTaskWithEtagAsync(int id)
    {
        using var scope = _logger.BeginScope("TaskId: {TaskId}", id);
        _logger.LogInformation("Inside:{Method}", nameof(GetTaskWithEtagAsync));

        var task = await _dbContext.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
        {
            return null;
        }

        var etag = await GetDbEtagAsync(id) ?? string.Empty;
        return (task, etag);
    }

    public async Task<UpdateTaskResult> UpdateTaskNameAsync(int taskId, string newName, string? ifMatch)
    {
        using var scope = _logger.BeginScope("TaskId: {TaskId}", taskId);
        _logger.LogInformation("Inside:{Method}", nameof(UpdateTaskNameAsync));

        try
        {
            var normalized = NormalizeIfMatch(ifMatch);
            if (string.IsNullOrEmpty(normalized))
            {
                // Missing precondition
                return new UpdateTaskResult(false, false, true, null);
            }

            // xmin is uint in PostgreSQL
            if (!uint.TryParse(normalized, out var etagXmin))
            {
                // Malformed If-Match
                return new UpdateTaskResult(false, false, true, null);
            }

            // Attach a stub entity and set original concurrency token (shadow 'xmin')
            var entity = new TaskModel { Id = taskId };
            _dbContext.Attach(entity);

            // Tell EF what we believe the current xmin is (from If-Match)
            _dbContext.Entry(entity).Property<uint>("xmin").OriginalValue = etagXmin;

            // Apply the change
            entity.Name = newName;
            _dbContext.Entry(entity).Property(e => e.Name).IsModified = true;

            // This will emit: UPDATE "Tasks" SET "Name" = @p WHERE "Id" = @id AND "xmin" = @orig
            var rows = await _dbContext.SaveChangesAsync();

            if (rows == 0)
            {
                // Very rare with concurrency tokens; fall back to distinguish 404 vs 412
                var exists = await _dbContext.Tasks.AsNoTracking().AnyAsync(t => t.Id == taskId);
                return exists
                    ? new UpdateTaskResult(false, false, true, null)     // 412 Precondition Failed
                    : new UpdateTaskResult(false, true, false, null);    // 404 Not Found
            }

            // Success → fetch fresh ETag
            var newEtag = await GetDbEtagAsync(taskId) ?? string.Empty;
            return new UpdateTaskResult(true, false, false, newEtag);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Concurrency conflict or missing row; check existence
            var exists = await _dbContext.Tasks.AsNoTracking().AnyAsync(t => t.Id == taskId);
            return exists
                ? new UpdateTaskResult(false, false, true, null)      // 412
                : new UpdateTaskResult(false, true, false, null);     // 404
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update task name.");
            return new UpdateTaskResult(false, false, false, null);
        }
    }
}
