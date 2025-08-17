using System.Text.Json.Serialization;

namespace Engine.Services.Clients.AccessorClient.Models;

public sealed class ChatMessage
{

    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid Id { get; set; }

    [JsonPropertyName("threadId")]
    public Guid ThreadId { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public MessageRole Role { get; set; } = MessageRole.User;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageRole
{
    User = 0,
    Assistant = 1,
    System = 2,
    Developer = 3,
    Tool = 4
}
