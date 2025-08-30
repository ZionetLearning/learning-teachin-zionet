using Dapr.Client;
using Manager.Models.QueueMessages;
using Manager.Routing;

namespace Manager.Services;

public class DaprQueueDispatcher : IQueueDispatcher
{
    private readonly DaprClient _dapr;
    private readonly IRoutingContextAccessor _routing;
    private readonly ILogger<DaprQueueDispatcher> _logger;

    public DaprQueueDispatcher(DaprClient dapr, IRoutingContextAccessor routing, ILogger<DaprQueueDispatcher> logger)
    {
        _dapr = dapr;
        _routing = routing;
        _logger = logger;
    }

    public async Task SendAsync(string queueName, Message message, CancellationToken ct = default)
    {
        var metadata = _routing.Current?.ToDictionary() ?? new Dictionary<string, string>();

        _logger.LogInformation(
                "[DISPATCH] Sending {Action} to {Queue} with Callback={Callback}, ReplyQueue={ReplyQueue}",
                message.ActionName,
                queueName,
                metadata.GetValueOrDefault("x-callback-method") ?? "(none)",
                metadata.GetValueOrDefault("x-callback-queue") ?? "(none)");

        //Extra log to dump ALL headers
        if (metadata.Count > 0)
        {
            _logger.LogInformation("[DISPATCH HEADERS] {Headers}",
                string.Join(", ", metadata.Select(kv => $"{kv.Key}={kv.Value}")));
        }

        await _dapr.InvokeBindingAsync(
            $"{queueName}-out",
            "create",
            message,
            metadata,
            ct
        );
    }
}