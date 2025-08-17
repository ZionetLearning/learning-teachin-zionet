using System.Text.Json.Serialization;

namespace Manager.Models.Chat;

public sealed class ChatThread
{
    [JsonPropertyName("threadId")]
    public Guid ThreadId { get; set; }

    [JsonPropertyName("userId")]
    public required string UserId { get; set; }

    [JsonPropertyName("chatName")]
    public string ChatName { get; set; } = string.Empty;

    [JsonPropertyName("chatType")]
    public string ChatType { get; set; } = "default";

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }
}
