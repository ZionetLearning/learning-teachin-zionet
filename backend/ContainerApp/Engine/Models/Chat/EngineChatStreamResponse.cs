namespace Engine.Models.Chat;
public sealed class EngineChatStreamResponse
{
    public required string RequestId { get; set; }

    public required Guid ThreadId { get; set; }

    public required Guid UserId { get; set; }

    public required string ChatName { get; set; }

    public int Sequence { get; set; }

    public string? Delta { get; set; }

    public string? ToolCall { get; set; }

    public string? ToolResult { get; set; }

    public bool IsFinal { get; set; }

    public ChatStreamStage Stage { get; set; }

    public long ElapsedMs { get; set; }
}
