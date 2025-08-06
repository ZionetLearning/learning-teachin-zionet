namespace Manager.Models;

public record IdempotencyRecord
{
    public required int TaskId { get; set; }
    public required string Status { get; set; } = default!;
    public required DateTime Timestamp { get; set; }
}