using Accessor.Constants;
using Accessor.Exceptions;
using Accessor.Models;
using Accessor.DB;
using Accessor.Services.Interfaces;
using Dapr.Client;

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
        var key = $"task:{id}";
        var stateEntry = await _daprClient.GetStateEntryAsync<TaskModel>(ComponentNames.StateStore, key);

        if (stateEntry.Value != null)
        {
            return stateEntry.Value;
        }

        var task = await _db.Tasks.FindAsync(id);
        if (task == null)
        {
            return null;
        }

        await _daprClient.SaveStateAsync(ComponentNames.StateStore, key, task,
            metadata: new Dictionary<string, string> { { "ttlInSeconds", _ttl.ToString() } });

        return task;
    }

    public async Task CreateTaskAsync(TaskModel task)
    {
        var key = $"task:{task.Id}";
        var cached = await _daprClient.GetStateAsync<TaskModel>(ComponentNames.StateStore, key);

        if (cached is not null && cached.Name != task.Name)
        {
            throw new ConflictException($"Task {task.Id} already exists with different payload.");
        }

        await _db.Tasks.AddAsync(task);
        await _db.SaveChangesAsync();

        await _daprClient.SaveStateAsync(ComponentNames.StateStore, key, task,
            metadata: new Dictionary<string, string> { { "ttlInSeconds", _ttl.ToString() } });
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
        var task = await _db.Tasks.FindAsync(taskId);

        if (task == null)
        {
            return false;
        }

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();

        var stateEntry = await _daprClient.GetStateEntryAsync<TaskModel>(ComponentNames.StateStore, $"task:{taskId}");
        if (stateEntry.Value != null)
        {
            await stateEntry.TryDeleteAsync();
        }

        return true;
    }
}