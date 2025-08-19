namespace Manager.Models.QueueMessages;

public record UserContextMetadata
{
    public required string UserId { get; set; }
    public string MessageId { get; set; } = string.Empty;
}