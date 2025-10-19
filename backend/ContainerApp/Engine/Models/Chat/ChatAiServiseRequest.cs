using Microsoft.SemanticKernel.ChatCompletion;

namespace Engine.Models.Chat;

public sealed record ChatAiServiceRequest
{
    public required string RequestId { get; init; }
    public required ChatHistory History { get; init; }
    public ChatType ChatType { get; init; } = ChatType.Default; // todo: use for systemPrompt
    public Guid ThreadId { get; init; }
    public required Guid UserId { get; init; }
    public required long SentAt { get; init; }
    public required int TtlSeconds { get; init; }
}
