namespace Manager.Models;

public class TaskUpdateMessage
{
    public int TaskId { get; set; }
    public required string Status { get; set; }
}