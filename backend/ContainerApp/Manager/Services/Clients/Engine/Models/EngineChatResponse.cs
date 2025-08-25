namespace Manager.Services.Clients.Engine.Models;

public sealed record EngineChatResponse
{
    public string? AssistantMessage { get; init; }

    public required string RequestId { get; init; }

    public required ChatAnswerStatus Status { get; set; }

    public Guid ThreadId { get; init; }
}

