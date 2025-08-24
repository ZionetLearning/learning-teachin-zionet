using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;

namespace Engine.Models.Chat;

public sealed class HistoryEnvelope
{
    [JsonPropertyName("messages")]
    public List<ChatMessageContent> Messages { get; set; } = new List<ChatMessageContent>();
}
