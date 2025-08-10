namespace Accessor.Models;

public class IdempotencyRecord
{
    public string IdempotencyKey { get; set; } = default!;
    public IdempotencyStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
}

public enum IdempotencyStatus { InProgress = 0, Completed = 1 }
