using Dapr.Client;
using Accessor.Constants;

namespace Accessor.Services;

public class ManagerCallbackQueueService : IManagerCallbackQueueService
{
    private readonly DaprClient _dapr;
    private readonly ILogger<ManagerCallbackQueueService> _logger;

    public ManagerCallbackQueueService(DaprClient dapr, ILogger<ManagerCallbackQueueService> logger)
    {
        _dapr = dapr;
        _logger = logger;
    }

    public async Task PublishToManagerCallbackAsync<T>(T message, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Publishing message to {QueueName}", QueueNames.ManagerCallbackQueue);
            await _dapr.InvokeBindingAsync($"{QueueNames.ManagerCallbackQueue}-out", "create", message, cancellationToken: ct);
            _logger.LogInformation("Message published to {QueueName}", QueueNames.ManagerCallbackQueue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to {QueueName}", QueueNames.ManagerCallbackQueue);
            throw;
        }
    }
}

