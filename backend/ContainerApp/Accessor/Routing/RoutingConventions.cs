using Accessor.Constants;
using Accessor.Models.QueueMessages;
using Accessor.Models;

namespace Accessor.Routing;

public static class RoutingConventions
{
    public const string DefaultReplyQueue = QueueNames.ManagerCallbackQueue;

    private static readonly Dictionary<MessageAction, string> _actionCallbacks = new()
    {
        [MessageAction.CreateTask] = "OnTaskCreatedAsync",
        [MessageAction.UpdateTask] = "OnTaskUpdatedAsync"
    };

    private static readonly Dictionary<TaskResultStatus, string> _resultCallbacks = new()
    {
        [TaskResultStatus.Created] = "OnTaskCreatedAsync",
        [TaskResultStatus.Updated] = "OnTaskUpdatedAsync"
    };

    // Gets the callback method name for a given action.
    public static string? GetCallbackForAction(MessageAction action) =>
        _actionCallbacks.TryGetValue(action, out var method) ? method : null;

    // Gets the callback method name for a given TaskResult status.
    public static string? GetCallbackForResult(TaskResultStatus status) =>
        _resultCallbacks.TryGetValue(status, out var method) ? method : null;
}
