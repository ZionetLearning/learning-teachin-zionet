namespace Accessor.Models;

public sealed record ChatMessageDto(
    Guid Id, Guid ThreadId, MessageRole Role, string UserId, string Content, DateTimeOffset Timestamp);

