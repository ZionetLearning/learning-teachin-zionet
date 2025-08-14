using System.Reflection;
using Microsoft.Extensions.DependencyInjection; 

namespace Common.Callbacks;

public class CallbackDispatcher
{
    private readonly ICallbackContextManager _ctxMgr;
    private readonly IServiceProvider _serviceProvider;

    public CallbackDispatcher(ICallbackContextManager ctxMgr, IServiceProvider serviceProvider)
    {
        _ctxMgr = ctxMgr;
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(object payload, IDictionary<string, string> headers)
    {
        var ctx = _ctxMgr.FromHeaders(headers);

        // Resolve target by type name
        var targetType = Type.GetType(ctx.TargetTypeName)
            ?? throw new InvalidOperationException($"Target type '{ctx.TargetTypeName}' not found.");

        var target = _serviceProvider.GetRequiredService(targetType);

        var method = targetType.GetMethod(ctx.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
            throw new InvalidOperationException($"Callback method '{ctx.MethodName}' not found.");

        var result = method.Invoke(target, new[] { payload });
        if (result is Task task) await task;
    }
}