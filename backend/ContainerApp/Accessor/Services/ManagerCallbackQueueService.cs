using Dapr.Client;
using Accessor.Constants;

namespace Accessor.Services;

public class ManagerCallbackQueueService : IManagerCallbackQueueService
{
    private readonly DaprClient _dapr;
    private readonly ILogger<ManagerCallbackQueueService> _logger;

    public ManagerCallbackQueueService(DaprClient dapr, ILogger<ManagerCallbackQueueService> logger)
    {
        _dapr = dapr ?? throw new ArgumentNullException(nameof(dapr));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishToManagerCallbackAsync<T>(T message, CancellationToken ct = default)
    {
        _logger.LogDebug("Publishing message to {QueueName}", QueueNames.ManagerCallbackQueue);
        await _dapr.InvokeBindingAsync($"{QueueNames.ManagerCallbackQueue}-out", "create", message, cancellationToken: ct);
        _logger.LogInformation("Message published to {QueueName}", QueueNames.ManagerCallbackQueue);
    }
}
