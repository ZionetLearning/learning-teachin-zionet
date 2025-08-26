
namespace Engine.Models.Chat;

public sealed record EngineChatRequest
{
    public required string RequestId { get; init; }

    public Guid ThreadId { get; init; } = Guid.NewGuid();

    public required string UserMessage { get; init; }

    public required ChatType ChatType { get; init; } = ChatType.Default;

    public required long SentAt { get; init; }

    public required int TtlSeconds { get; init; }
}