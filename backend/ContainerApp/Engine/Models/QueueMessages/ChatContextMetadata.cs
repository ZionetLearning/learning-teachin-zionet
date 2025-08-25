namespace Engine.Models.QueueMessages;

public sealed record ChatContextMetadata
{
    public required Guid ThreadId { get; init; }
    public required string UserId { get; init; } = string.Empty;
    public required string RequestId { get; init; } = string.Empty;
}
