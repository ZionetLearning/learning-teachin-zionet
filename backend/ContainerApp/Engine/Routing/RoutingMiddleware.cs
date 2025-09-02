using Engine.Models.QueueMessages;
using Engine.Models;
//using Engine.Constants;

namespace Engine.Routing;

public class RoutingMiddleware
{
    private readonly IRoutingContextAccessor _accessor;
    private readonly ILogger<RoutingMiddleware> _logger;
    public IRoutingContextAccessor Accessor => _accessor;

    public RoutingMiddleware(IRoutingContextAccessor accessor, ILogger<RoutingMiddleware> logger)
    {
        _accessor = accessor;
        _logger = logger;
    }

    public async Task HandleAsync(
        Message message,
        IReadOnlyDictionary<string, string>? metadata,
        Func<Task> next)
    {
        TaskResult? result;
        var ctx = new RoutingContext
        {
            ReplyQueue =
                metadata?.GetValueOrDefault("x-callback-queue")
                ?? RoutingConventions.DefaultReplyQueue,

            CallbackMethod =
                metadata?.GetValueOrDefault("x-callback-method")
                ?? RoutingConventions.GetCallbackForAction(message.ActionName)
                ?? (RoutingContextExtensions.TryDeserializeTaskResult(message.Payload, out result) && result != null
                    ? RoutingConventions.GetCallbackForResult(result.Status)
                    : null)
        };

        _logger.LogInformation(
            "[ROUTING] Established context → ReplyQueue={ReplyQueue}, CallbackMethod={CallbackMethod}",
            ctx.ReplyQueue ?? "(none)",
            ctx.CallbackMethod ?? "(none)");

        _accessor.Current = ctx;
        try
        {
            await next();
        }
        finally
        {
            _accessor.Current = null;
        }
    }
}