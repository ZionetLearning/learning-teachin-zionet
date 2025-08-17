using Engine.Services.Clients.AccessorClient.Models;

namespace Engine.Models.Chat;

public sealed record ChatAiServiseRequest
{
    public required string RequestId { get; init; }
    public IReadOnlyList<ChatMessage> History { get; init; } = Array.Empty<ChatMessage>();
    public string UserMessage { get; init; } = string.Empty;
    public ChatType ChatType { get; init; } = ChatType.Default; // todo: use for systemPrompt
    public Guid ThreadId { get; init; }
    public required string UserId { get; init; }
    public required long SentAt { get; init; }
    public required int TtlSeconds { get; init; }
}
