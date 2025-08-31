using System.Text.Json.Serialization;

namespace Manager.Models.Chat;

public sealed record AIChatResponse
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; init; }

    [JsonPropertyName("assistantMessage")]
    public string? AssistantMessage { get; init; }

    [JsonPropertyName("chatName")]
    public required string ChatName { get; init; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChatAnswerStatus Status { get; set; } = ChatAnswerStatus.Ok;

    [JsonPropertyName("threadId")]
    public Guid ThreadId { get; init; }
}