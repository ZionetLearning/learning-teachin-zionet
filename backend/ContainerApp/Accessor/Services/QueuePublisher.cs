using Dapr.Client;

namespace Accessor.Services;

public class QueuePublisher : IQueuePublisher
{
    private readonly DaprClient _dapr;
    private readonly ILogger<QueuePublisher> _logger;

    public QueuePublisher(DaprClient dapr, ILogger<QueuePublisher> logger)
    {
        _dapr = dapr ?? throw new ArgumentNullException(nameof(dapr));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync<T>(string queueName, T message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException("Queue name is required", nameof(queueName));
        }

        _logger.LogDebug("Publishing message to queue {Queue}", queueName);
        await _dapr.InvokeBindingAsync(queueName, "create", message, cancellationToken: ct);
        _logger.LogInformation("Message published to queue {Queue}", queueName);
    }
}
