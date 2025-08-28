namespace Manager.Services;

public static class CallbackMetadataFactory
{
    private const string DefaultQueue = "manager-callback-queue";

    public static IReadOnlyDictionary<string, string> GetCallbackMethod(string methodName)
    {
        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException("Method name cannot be null, empty, or whitespace.", nameof(methodName));
        }

        return CallbackHeaderHelper.Create(DefaultQueue, methodName);
    }

    public static IReadOnlyDictionary<string, string> GetCallbackMetadata(string queue, string methodName)
    {
        if (string.IsNullOrWhiteSpace(queue))
        {
            throw new ArgumentException("Queue name cannot be null, empty, or whitespace.", nameof(queue));
        }

        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException("Method name cannot be null, empty, or whitespace.", nameof(methodName));
        }

        return CallbackHeaderHelper.Create(queue, methodName);
    }
}