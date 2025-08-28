namespace Engine.Models.Chat;

public sealed record ChatHistoryForFrontDto
{
    public required Guid ChatId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ChatType { get; init; } = "default";
    public required IReadOnlyList<ChatHistoryMessageDto> Messages { get; init; }
}