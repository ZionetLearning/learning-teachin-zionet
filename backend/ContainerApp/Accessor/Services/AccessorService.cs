﻿using Accessor.Constants;
using Accessor.DB;
using Accessor.Models;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using Accessor.Models.Users;
using Accessor.Models.Auth;
using Accessor.Exceptions;

namespace Accessor.Services;
public class AccessorService : IAccessorService
{
    private readonly ILogger<AccessorService> _logger;
    private readonly AccessorDbContext _dbContext;
    private readonly DaprClient _daprClient;
    private readonly IConfiguration _configuration;
    private readonly int _ttl;

    public AccessorService(AccessorDbContext dbContext,
        ILogger<AccessorService> logger,
        DaprClient daprClient,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _logger = logger;
        _daprClient = daprClient;
        _configuration = configuration;
        _ttl = int.Parse(configuration["TaskCache:TTLInSeconds"] ??
                                                  throw new KeyNotFoundException("TaskCache:TTLInSeconds is not configured"));
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
        {
            _logger.LogInformation("Inside:{Method}", nameof(GetTaskByIdAsync));
            try
            {
                var key = GetTaskCacheKey(id);

                // Try to get from Redis cache
                var stateEntry = await _daprClient.GetStateEntryAsync<TaskModel>(ComponentNames.StateStore, key);

                if (stateEntry.Value != null)
                {
                    _logger.LogInformation("Task found in Redis cache (ETag: {ETag})", stateEntry.ETag);
                    return stateEntry.Value;
                }

                // If not found in cache, fetch from database
                var task = await _dbContext.Tasks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    _logger.LogWarning("Task not found.");
                    return null;
                }

                // Save the task to Redis cache with ETag, if its not already cached
                stateEntry.Value = task;

                await _daprClient.SaveStateAsync(
                    storeName: ComponentNames.StateStore,
                    key: key,
                    value: task,
                    metadata: new Dictionary<string, string>
                    {
                    { "ttlInSeconds", _ttl.ToString() }
                    }
                );

                _logger.LogInformation("Fetched task from DB and cached");
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task");
                throw;
            }
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
            // 1. Check cache for idempotency
            var cached = await _daprClient.GetStateAsync<TaskModel>(
                ComponentNames.StateStore,
                GetTaskCacheKey(task.Id));

            if (cached is not null)
            {
                if (!TasksAreEqual(cached, task))
                {
                    _logger.LogWarning("Conflict: Task {TaskId} already exists with different payload", task.Id);
                    throw new ConflictException($"Task {task.Id} already exists with different payload.");
                }

                _logger.LogInformation("Idempotent retry for Task {TaskId}, no-op", task.Id);
                return; // same payload → idempotent success
            }

            // 2. Insert new task
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} persisted", task.Id);

            // 3. Cache for retries
            await _daprClient.SaveStateAsync(
                storeName: ComponentNames.StateStore,
                key: GetTaskCacheKey(task.Id),
                value: task,
                stateOptions: null,
                metadata: new Dictionary<string, string> { { "ttlInSeconds", _ttl.ToString() } });

            _logger.LogInformation("Cached task {TaskId} with TTL {TTL}s", task.Id, _ttl);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Duplicate Task insert for {TaskId}", task.Id);

            var dbTask = await _dbContext.Tasks.FindAsync(task.Id);
            if (dbTask is null)
            {
                throw;
            }

            if (!TasksAreEqual(dbTask, task))
            {
                _logger.LogWarning("Conflict: Task {TaskId} already exists in DB with different payload", task.Id);
                throw new ConflictException($"Task {task.Id} already exists with different payload.");
            }

            // Ensure consistent cache
            await _daprClient.SaveStateAsync(
                storeName: ComponentNames.StateStore,
                key: GetTaskCacheKey(task.Id),
                value: dbTask,
                stateOptions: null,
                metadata: new Dictionary<string, string> { { "ttlInSeconds", _ttl.ToString() } });

            // Idempotent no-op
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

    public async Task<bool> UpdateTaskNameAsync(int taskId, string newName)
    {
        using var scope = _logger.BeginScope("TaskId: {TaskId}", taskId);
        {
            _logger.LogInformation("Inside:{Method}", nameof(UpdateTaskNameAsync));
            try
            {
                var task = await _dbContext.Tasks.FindAsync(taskId);
                if (task == null)
                {
                    _logger.LogWarning("Task not found in DB.");
                    return false;
                }

                task.Name = newName;
                await _dbContext.SaveChangesAsync();

                var key = GetTaskCacheKey(taskId);

                // Load current state from Redis
                var stateEntry = await _daprClient.GetStateEntryAsync<TaskModel>(ComponentNames.StateStore, key);

                if (stateEntry.Value == null)
                {
                    _logger.LogWarning("Task not found in Redis cache. Skipping cache update.");
                    return true;
                }

                stateEntry.Value.Name = newName;

                await _daprClient.SaveStateAsync(
                   storeName: ComponentNames.StateStore,
                   key: key,
                   value: task,
                   metadata: new Dictionary<string, string>
                   {
                    { "ttlInSeconds", _ttl.ToString() }
                   });
                _logger.LogInformation("Successfully cached task with TTL {TTL}s", _ttl);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update task name.");
                return false;
            }
        }
    }

    public async Task<bool> DeleteTaskAsync(int taskId)
    {
        using var scope = _logger.BeginScope("TaskId: {TaskId}", taskId);
        {
            _logger.LogInformation("Inside:{Method}", nameof(DeleteTaskAsync));
            try
            {
                var task = await _dbContext.Tasks.FindAsync(taskId);
                if (task == null)
                {
                    _logger.LogWarning("Task not found in DB.");
                    return false;
                }
                // Remove task from the database
                _dbContext.Tasks.Remove(task);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Task deleted from DB.");

                // Remove task from Redis cache
                var key = GetTaskCacheKey(taskId);
                var stateEntry = await _daprClient.GetStateEntryAsync<TaskModel>(ComponentNames.StateStore, key);

                if (stateEntry.Value == null)
                {
                    _logger.LogWarning("Task not found in Redis cache. Skipping cache deletion.");
                    return true;
                }

                var deleteSuccess = await stateEntry.TryDeleteAsync();

                if (deleteSuccess)
                {
                    _logger.LogInformation("Task removed from Redis cache (ETag: {ETag})", stateEntry.ETag);
                }
                else
                {
                    _logger.LogWarning("ETag conflict — failed to remove task from Redis cache.");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete task.");
                return false;
            }
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
                Role = user.Role
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
                    Role = u.Role
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
}
