namespace Engine.Models.Chat;

public sealed record ChatHistoryMessageDto
{
    public string Role { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty;

    public DateTimeOffset? CreatedAt { get; init; }
}