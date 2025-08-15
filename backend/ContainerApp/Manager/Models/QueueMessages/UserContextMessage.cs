namespace Manager.Models.QueueMessages;

public record UserContextMessage : Message
{
    public required string UserId { get; set; }
    public string MessageId { get; set; } = string.Empty;
}