using System.Text.Json.Serialization;

namespace Manager.Models.Chat;

public class OpenAiMessageDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<OpenAiToolCallDto>? ToolCalls { get; set; }

    [JsonPropertyName("tool_call_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToolCallId { get; set; }
}

public class OpenAiToolCallDto
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public OpenAiFunctionDto? Function { get; set; }
}

public class OpenAiFunctionDto
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }
}
