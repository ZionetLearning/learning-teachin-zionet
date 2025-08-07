namespace Manager.Constants;

public static class QueueNames
{
    public const string ManagerToEngine = "manager-to-engine";
    public const string TaskUpdate = "taskupdate";
    public const string ManagerToAi = "manager-to-ai";
    public const string AiToManager = "ai-to-manager";


    // new queue names
    public const string ManagerCallbackQueue = "manager-callback-queue";
    public const string AccessorQueue = "accessor-queue";
    public const string EngineQueue = "engine-queue";
}
