namespace Engine.Services;

public static class CallbackHeaderHelper
{
    public const string HeaderQueue = "x-callback-queue";
    public const string HeaderMethod = "x-callback-method";

    public static Dictionary<string, string> Create(string queueName, string methodName)
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException("Queue name cannot be empty", nameof(queueName));
        }

        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException("Method name cannot be empty", nameof(methodName));
        }

        return new Dictionary<string, string>
        {
            [HeaderQueue] = queueName,
            [HeaderMethod] = methodName
        };
    }
}
