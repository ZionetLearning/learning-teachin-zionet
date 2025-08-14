namespace Common.Callbacks;

public class CallbackContextManager : ICallbackContextManager
{
    public IDictionary<string, string> ToHeaders(CallbackContext context) =>
        new Dictionary<string, string>
        {
            ["x-callback-target"] = context.TargetTypeName,
            ["x-callback-method"] = context.MethodName,
            ["x-callback-queue"]  = context.QueueName
        };

    public CallbackContext FromHeaders(IDictionary<string, string> headers) =>
        new CallbackContext(
            headers.TryGetValue("x-callback-target", out var t) ? t : string.Empty,
            headers.TryGetValue("x-callback-method", out var m) ? m : string.Empty,
            headers.TryGetValue("x-callback-queue", out var q) ? q : string.Empty
        );
}
