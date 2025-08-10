using System.Text.Json.Serialization;

namespace Accessor.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageRole
{
    User,
    Assistant,
    System,
    Developer,
    Tool
}

