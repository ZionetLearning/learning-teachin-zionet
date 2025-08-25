namespace Manager.Models.Chat;

public sealed record AIChatResponse
{
    public required string RequestId { get; init; }

    public string? AssistantMessage { get; init; }

    public ChatAnswerStatus Status { get; set; } = ChatAnswerStatus.Ok;

    public Guid ThreadId { get; init; }
}