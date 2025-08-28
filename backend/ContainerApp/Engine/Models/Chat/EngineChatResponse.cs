namespace Engine.Models.Chat;

public sealed record EngineChatResponse
{
    public required string RequestId { get; init; }

    public string? AssistantMessage { get; init; }
    public required string ChatName { get; init; }

    public ChatAnswerStatus Status { get; set; } = ChatAnswerStatus.Ok;

    public Guid ThreadId { get; init; }
}

