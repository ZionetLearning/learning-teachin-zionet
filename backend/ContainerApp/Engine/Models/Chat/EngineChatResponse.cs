namespace Engine.Models.Chat;

public sealed record EngineChatResponse
{
    public required string RequestId { get; init; }

    public string? AssistantMessage { get; init; }

    public string Status { get; set; } = "ok";

    public Guid ThreadId { get; init; }
}

