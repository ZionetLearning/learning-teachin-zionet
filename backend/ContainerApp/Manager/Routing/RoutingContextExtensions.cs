using Manager.Models;
using System.Text.Json;

namespace Manager.Routing;

public static class RoutingContextExtensions
{
    public static IReadOnlyDictionary<string, string>? ToDictionary(this RoutingContext? ctx)
    {
        if (ctx is null)
        {
            return null;
        }

        var dict = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(ctx.ReplyQueue))
        {
            dict["x-callback-queue"] = ctx.ReplyQueue;
        }

        if (!string.IsNullOrEmpty(ctx.CallbackMethod))
        {
            dict["x-callback-method"] = ctx.CallbackMethod;
        }

        return dict;
    }

    public static bool TryDeserializeTaskResult(JsonElement element, out TaskResult? result)
    {
        try
        {
            result = element.Deserialize<TaskResult>();
            return result != null;
        }
        catch
        {
            result = null!;
            return false;
        }
    }
}