namespace Accessor.Models;

public class TaskCacheOptions
{
    public int TTLInSeconds { get; set; }
    public int MaxRetries { get; set; }
    public int RetryBackoffMs { get; set; }
}
