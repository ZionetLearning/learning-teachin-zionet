namespace Manager.Models.Chat;

public class AIChatStreamResponse
{
    public string RequestId { get; set; } = string.Empty;
    public Guid ThreadId { get; set; }
    public Guid UserId { get; set; }
    public string ChatName { get; set; } = string.Empty;
    public string? Delta { get; set; }
    public int Sequence { get; set; }
    public string Stage { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public long ElapsedMs { get; set; }
    public string? ToolCall { get; set; }
    public string? ToolResult { get; set; }
}
