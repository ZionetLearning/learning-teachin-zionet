using Accessor.Messaging;
using Accessor.Models;
using Accessor.Services;

namespace Accessor.Endpoints;
public class AccessorQueueHandler : IQueueHandler<AccessorPayload>
{
    private readonly IAccessorService _accessorService;
    private readonly ILogger<AccessorQueueHandler> _logger;

    public AccessorQueueHandler(
        IAccessorService accessorService,
        ILogger<AccessorQueueHandler> logger)
    {
        _accessorService = accessorService;
        _logger = logger;
    }

    // add error throw
    public async Task HandleAsync(AccessorPayload msg, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        // now we need to get the payload, check the action, and call the appropriate service method
        // switch-case
        try
        {
            if (msg == null)
            {
                _logger.LogWarning("Received null message");
                return;
            }

            _logger.LogDebug("Queue→CreateTask {Id}", msg.Id);
            var model = new TaskModel
            {
                Id = msg.Id,
                Name = msg.Name,
                Payload = msg.Payload,
            };

            switch (msg.ActionName)
            {
                case "CreateTask":
                    await _accessorService.CreateTaskAsync(model);
                    break;

                case "UpdateTask":
                    await _accessorService.UpdateTaskNameAsync(model.Id, model.Name);
                    break;

                default:
                    _logger.LogWarning("Unknown action {ActionName} for Task {Id}", msg.ActionName, msg.Id);
                    return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message {Id}", msg.Id);
            throw;
        }
    }
}