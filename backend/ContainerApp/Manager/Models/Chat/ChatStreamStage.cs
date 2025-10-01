using System.Text.Json.Serialization;

namespace Manager.Models.Chat;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatStreamStage
{
    Unknown = 0,
    Model = 1,
    Tool = 2,
    ToolResult = 3,
    Final = 4,
    Canceled = 5,
    Expired = 6
}
