using Accessor.Constants;
using Accessor.DB;
using Accessor.Exceptions;
using Accessor.Models;
using Accessor.Services.Interfaces;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class TaskService : ITaskService
{
    private readonly ILogger<TaskService> _logger;
    private readonly AccessorDbContext _db;
    private readonly DaprClient _daprClient;
    private readonly int _ttl;

    public TaskService(AccessorDbContext db, ILogger<TaskService> logger,
        DaprClient daprClient, IConfiguration configuration)
    {
        _db = db;
        _logger = logger;
        _daprClient = daprClient;
        _ttl = int.Parse(configuration["TaskCache:TTLInSeconds"] ??
                         throw new KeyNotFoundException("TaskCache:TTLInSeconds is not configured"));
    }

    public async Task<TaskModel?> GetTaskByIdAsync(int id)
    {
        using var scope = _logger.BeginScope("TaskId: {TaskId}", id);
        _logger.LogInformation("Inside:{Method}", nameof(GetTaskByIdAsync));

        try
        {
            return await _db.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task");
            throw;
        }
    }

    public async Task CreateTaskAsync(TaskModel task)
    {
        using var scope = _logger.BeginScope("TaskId: {TaskId}", task.Id);
        _logger.LogInformation("Inside:{Method}", nameof(CreateTaskAsync));

        try
        {
            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Task {TaskId} persisted", task.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Duplicate Task insert for {TaskId}", task.Id);

            var dbTask = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == task.Id);
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

    public async Task<bool> UpdateTaskNameAsync(int taskId, string newName)
    {
        var task = await _db.Tasks.FindAsync(taskId);

        if (task == null)
        {
            return false;
        }

        task.Name = newName;
        await _db.SaveChangesAsync();

        await _daprClient.SaveStateAsync(ComponentNames.StateStore, $"task:{taskId}", task,
            metadata: new Dictionary<string, string> { { "ttlInSeconds", _ttl.ToString() } });

        return true;
    }

    public async Task<bool> DeleteTaskAsync(int taskId)
    {
        using var scope = _logger.BeginScope("TaskId: {TaskId}", taskId);
        _logger.LogInformation("Inside:{Method}", nameof(DeleteTaskAsync));

        try
        {
            var task = await _db.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return false;
            }

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete task.");
            return false;
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
        var xmin = await _db.Tasks
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

        var task = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
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
                return new UpdateTaskResult(false, false, true, null);
            }

            // xmin is uint in PostgreSQL
            if (!uint.TryParse(normalized, out var etagXmin))
            {
                return new UpdateTaskResult(false, false, true, null);
            }

            var entity = new TaskModel { Id = taskId };
            _db.Attach(entity);

            _db.Entry(entity).Property<uint>("xmin").OriginalValue = etagXmin;

            entity.Name = newName;
            _db.Entry(entity).Property(e => e.Name).IsModified = true;

            var rows = await _db.SaveChangesAsync();

            if (rows == 0)
            {
                var exists = await _db.Tasks.AsNoTracking().AnyAsync(t => t.Id == taskId);
                return exists
                    ? new UpdateTaskResult(false, false, true, null)
                    : new UpdateTaskResult(false, true, false, null);
            }

            var newEtag = await GetDbEtagAsync(taskId) ?? string.Empty;
            return new UpdateTaskResult(true, false, false, newEtag);
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _db.Tasks.AsNoTracking().AnyAsync(t => t.Id == taskId);
            return exists
                ? new UpdateTaskResult(false, false, true, null)
                : new UpdateTaskResult(false, true, false, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update task name.");
            return new UpdateTaskResult(false, false, false, null);
        }
    }
}