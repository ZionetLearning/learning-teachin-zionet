using System.Text.Json.Serialization;

namespace IntegrationTests.Models;

public class ChatThreadDto
{
    [JsonPropertyName("threadId")]
    public Guid ThreadId { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = "";

    [JsonPropertyName("chatType")]
    public string ChatType { get; set; } = "default";

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }
}
