namespace Manager.Constants;

public static class QueueNames
{
    public const string ManagerCallbackQueue = "manager-callback-queue";
    public const string ManagerCallbackSessionQueue = "manager-callback-session-queue"; // new queue for session related callbacks
    public const string EngineQueue = "engine-queue";
    public const string AccessorQueue = "accessor-queue";
}
