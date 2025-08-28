namespace Manager.Services.Clients.Engine.Models;

public sealed record ChatHistoryForFrontDto
{
    public required Guid ChatId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ChatType { get; init; } = "default";
    public required IReadOnlyList<ChatHistoryMessageDto> Messages { get; init; }
}

public sealed record ChatHistoryMessageDto
{
    public string Role { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty;

    public DateTimeOffset? CreatedAt { get; init; }
}