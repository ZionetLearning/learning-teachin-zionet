using Accessor.Models;
using Accessor.Services;

namespace Accessor.Messaging;

public class AccessorCreateTaskHandler : IQueueHandler<TaskModel>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AccessorCreateTaskHandler> _logger;

    public AccessorCreateTaskHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<AccessorCreateTaskHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task HandleAsync(TaskModel msg, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Queue→CreateTask {Id}", msg.Id);

        using var scope = _scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IAccessorService>();

        await svc.CreateTaskAsync(msg);

        _logger.LogInformation("Created Task {Id}", msg.Id);
    }
}