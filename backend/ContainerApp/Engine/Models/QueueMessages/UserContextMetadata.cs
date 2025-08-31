namespace Engine.Models.QueueMessages;

public record UserContextMetadata
{
    public required string UserId { get; init; }
    public string MessageId { get; set; } = string.Empty;
}
