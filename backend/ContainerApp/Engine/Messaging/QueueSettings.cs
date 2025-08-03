namespace Engine.Messaging;

public class QueueSettings
{
    public int MaxConcurrentCalls { get; set; } = 1;
    public int PrefetchCount { get; set; }
    public int ProcessingDelayMs { get; set; }
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
}
