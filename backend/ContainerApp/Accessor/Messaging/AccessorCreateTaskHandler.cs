using Accessor.Models;
using Accessor.Services;

namespace Accessor.Messaging;
public class AccessorCreateTaskHandler : IQueueHandler<TaskModel>
{
    private readonly IAccessorService _svc;
    private readonly ILogger<AccessorCreateTaskHandler> _log;
    public AccessorCreateTaskHandler(IAccessorService svc, ILogger<AccessorCreateTaskHandler> log)
    {
        _svc = svc; _log = log;
    }
    public async Task HandleAsync(TaskModel msg, CancellationToken ct)
    {
        _log.LogDebug("Queue→CreateTask {Id}", msg.Id);
        await _svc.CreateTaskAsync(msg);
        _log.LogInformation("Created Task {Id}", msg.Id);
    }
}


public class AccessorUpdateTaskNameHandler : IQueueHandler<UpdateTaskName>
{
    private readonly IAccessorService _svc;
    private readonly ILogger<AccessorUpdateTaskNameHandler> _log;
    public AccessorUpdateTaskNameHandler(IAccessorService svc, ILogger<AccessorUpdateTaskNameHandler> log)
    {
        _svc = svc; _log = log;
    }
    public async Task HandleAsync(UpdateTaskName msg, CancellationToken ct)
    {
        _log.LogDebug("Queue→UpdateName {Id}", msg.Id);
        await _svc.UpdateTaskNameAsync(msg.Id, msg.Name);
        _log.LogInformation("Updated Task {Id} name to {Name}", msg.Id, msg.Name);
    }
}