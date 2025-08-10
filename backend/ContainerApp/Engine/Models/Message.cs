using System.Text.Json;
using System.Text.Json.Serialization;

namespace Engine.Models;

public record Message
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageAction ActionName { get; set; }
    public JsonElement Payload { get; set; }
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageAction
{
    CreateTask,
    TestLongTask
}