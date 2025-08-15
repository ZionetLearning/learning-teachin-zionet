using System.Text.Json.Serialization;

namespace Accessor.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageRole
{
    User = 0,
    Assistant = 1,
    System = 2,
    Developer = 3,
    Tool = 4
}

