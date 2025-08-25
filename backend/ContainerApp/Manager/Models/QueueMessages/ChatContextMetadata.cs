namespace Manager.Models.QueueMessages;

public sealed record ChatContextMetadata
{
    public Guid? ThreadId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
}
