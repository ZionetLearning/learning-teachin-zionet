using System.Text.Json.Serialization;

namespace Engine.Models.Chat;

public sealed class SnapshotDto
{
    public List<MessageDto> Messages { get; init; } = new();
}

public sealed class MessageDto
{
    public string Role { get; init; } = "";
    public string? Content { get; init; }
    public long? CreatedAt { get; init; }
    public UsageDto? Usage { get; init; }
    public string? Model { get; init; }
    public string? FinishReason { get; init; }
    public Dictionary<string, string?>? MetadataDump { get; init; }
    public List<ItemDto> Items { get; init; } = new();
}

public sealed class UsageDto
{
    public int? PromptTokens { get; init; }
    public int? CompletionTokens { get; init; }
    public int? TotalTokens { get; init; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextItemDto), "text")]
[JsonDerivedType(typeof(ToolCallItemDto), "toolCall")]
[JsonDerivedType(typeof(ToolResultItemDto), "toolResult")]
public abstract class ItemDto { }

public sealed class TextItemDto : ItemDto
{
    public string? Text { get; init; }
}
public sealed class ToolCallItemDto : ItemDto
{
    public string? Name { get; init; }
    public string? Arguments { get; init; }
    public string? Id { get; init; }
}
public sealed class ToolResultItemDto : ItemDto
{
    public string? Name { get; init; }
    public string? Result { get; init; }
    public string? Id { get; init; }
}