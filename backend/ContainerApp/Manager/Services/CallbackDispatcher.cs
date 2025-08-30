using System.Reflection;
using System.Text.Json;
using Manager.Models;
using Manager.Models.QueueMessages;
using Manager.Routing;

namespace Manager.Services;

public class CallbackDispatcher : ICallbackDispatcher
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CallbackDispatcher> _logger;
    private readonly IRoutingContextAccessor _routing;

    public CallbackDispatcher(
        IServiceProvider services,
        ILogger<CallbackDispatcher> logger,
        IRoutingContextAccessor routing)
    {
        _services = services;
        _logger = logger;
        _routing = routing;
    }

    public async Task DispatchAsync(Message message, CancellationToken ct)
    {
        var result = message.Payload.Deserialize<TaskResult>();
        if (result == null)
        {
            _logger.LogWarning("[CALLBACK] TaskResult deserialization failed for {Action}", message.ActionName);
            return;
        }

        var ctx = _routing.Current;
        var callbackName = ctx?.CallbackMethod;

        _logger.LogInformation("[CALLBACK] Dispatching {Status} for Task {TaskId}", result.Status, result.Id);

        var callbacks = _services.GetService<IManagerCallbacks>();
        if (callbacks == null)
        {
            _logger.LogWarning("[CALLBACK] No IManagerCallbacks service registered.");
            return;
        }

        if (string.IsNullOrEmpty(callbackName))
        {
            _logger.LogWarning("[CALLBACK] No callback method in RoutingContext.");
            return;
        }

        //Use reflection to find matching method on IManagerCallbacks
        var method = typeof(IManagerCallbacks).GetMethod(callbackName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

        if (method == null)
        {
            _logger.LogWarning("[CALLBACK] Method {CallbackMethod} not found on IManagerCallbacks.", callbackName);
            return;
        }

        _logger.LogInformation("[CALLBACK] Invoking {CallbackMethod} for Task {TaskId}", callbackName, result.Id);

        var task = (Task?)method.Invoke(callbacks, new object?[] { result });
        if (task != null)
        {
            await task;
        }
    }
}