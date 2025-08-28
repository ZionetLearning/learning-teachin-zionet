using System.Text.Json;
using Manager.Models.QueueMessages;

namespace Manager.Services;

public class CallbackDispatcher : ICallbackDispatcher
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CallbackDispatcher> _logger;

    public CallbackDispatcher(IServiceProvider services, ILogger<CallbackDispatcher> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task DispatchAsync(Message message, IReadOnlyDictionary<string, string>? metadataCallback, CancellationToken ct)
    {
        string? target = null;
        metadataCallback?.TryGetValue(CallbackHeaderHelper.HeaderMethod, out target);

        if (string.IsNullOrEmpty(target))
        {
            _logger.LogWarning("No callback method in metadataCallbacks for {Action}", message.ActionName);
            return;
        }

        var callback = _services.GetService<IManagerCallbacks>();
        var method = typeof(IManagerCallbacks).GetMethod(target);

        if (callback == null || method == null)
        {
            _logger.LogWarning("No callback service found for {Target}", target);
            return;
        }

        _logger.LogInformation("Dispatching callback {Target} on {Service}", target, callback.GetType().Name);

        var paramType = method.GetParameters().First().ParameterType;
        var model = message.Payload.Deserialize(paramType);

        var task = (Task?)method.Invoke(callback, new[] { model! });
        if (task != null)
        {
            await task;
        }
    }
}