using System.ComponentModel.DataAnnotations;

namespace Accessor.Models;

public class TaskCacheOptions
{
    [Range(1, int.MaxValue, ErrorMessage = "TTLInSeconds must be positive.")]
    public int TTLInSeconds { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "MaxRetries must be non-negative.")]
    public int MaxRetries { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "RetryBackoffMs must be positive.")]
    public int RetryBackoffMs { get; set; }
}