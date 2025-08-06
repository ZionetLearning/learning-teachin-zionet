using Accessor.Models;
using Accessor.Services;

namespace Accessor.Messaging;

public class AccessorUpdateTaskNameHandler : IQueueHandler<UpdateTaskName>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AccessorUpdateTaskNameHandler> _logger;

    public AccessorUpdateTaskNameHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<AccessorUpdateTaskNameHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateTaskName msg, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Queue→UpdateName {Id}", msg.Id);

        // open a new scope so we can safely resolve scoped services
        using var scope = _scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IAccessorService>();

        await svc.UpdateTaskNameAsync(msg.Id, msg.Name);

        _logger.LogInformation(
            "Updated Task {Id} name to {Name}",
            msg.Id,
            msg.Name
        );
    }
}
