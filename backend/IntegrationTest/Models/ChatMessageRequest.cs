using System.Text.Json.Serialization;

namespace IntegrationTests.Models;

public class ChatMessageRequest
{
    [JsonPropertyName("threadId")]
    public Guid ThreadId { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = "";

    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}
