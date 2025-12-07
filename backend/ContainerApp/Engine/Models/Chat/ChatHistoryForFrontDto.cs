
namespace Engine.Models.Chat;

public class ChatHistoryForFrontDto
{
    public required Guid ChatId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ChatType { get; init; } = "default";
    public IReadOnlyList<OpenAiMessageDto>? Messages { get; init; }
}