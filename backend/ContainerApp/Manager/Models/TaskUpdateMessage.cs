namespace Manager.Models;

public record TaskUpdateMessage
{
    public int TaskId { get; set; }
    public required string Status { get; set; }
}