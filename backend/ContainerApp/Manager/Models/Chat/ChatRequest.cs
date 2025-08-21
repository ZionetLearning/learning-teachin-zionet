using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Manager.Models.Chat;

public sealed record ChatRequest
{
    [Required, MinLength(36)]
    [JsonPropertyName("threadId")]
    public string ThreadId { get; init; } = string.Empty;

    [Required, MinLength(1)]
    [JsonPropertyName("userMessage")]
    public string UserMessage { get; init; } = string.Empty;

    [JsonPropertyName("chatType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChatType ChatType { get; init; } = ChatType.Default;

}