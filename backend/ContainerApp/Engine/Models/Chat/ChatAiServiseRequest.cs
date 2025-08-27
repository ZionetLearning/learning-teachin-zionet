using System.Text.Json;

namespace Engine.Models.Chat;

public sealed record ChatAiServiseRequest
{
    public required string RequestId { get; init; }
    public required JsonElement History { get; init; }
    public string UserMessage { get; init; } = string.Empty;
    public ChatType ChatType { get; init; } = ChatType.Default; // todo: use for systemPrompt
    public Guid ThreadId { get; init; }
    public required Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public required long SentAt { get; init; }
    public required int TtlSeconds { get; init; }
}
