using System.Text.Json;
using System.Text.Json.Serialization;

namespace Accessor.Models;

public record Message
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageAction ActionName { get; set; }
    public JsonElement Payload { get; set; }
    public IDictionary<string, string>? Metadata { get; set; }
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageAction
{
    UpdateTask,
    TaskResult
}