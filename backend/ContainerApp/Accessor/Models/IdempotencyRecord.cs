namespace Accessor.Models;

public class IdempotencyRecord
{
    public required string IdempotencyKey { get; set; }
    public IdempotencyStatus Status { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
}

public enum IdempotencyStatus { InProgress = 0, Completed = 1 }
