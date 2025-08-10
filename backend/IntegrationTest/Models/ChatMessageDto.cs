using System.Text.Json.Serialization;

namespace IntegrationTests.Models;

public class ChatMessageDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("threadId")]
    public Guid ThreadId { get; set; }

    [JsonPropertyName("thread")]
    public ChatThreadDto? Thread { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = "";

    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
}
