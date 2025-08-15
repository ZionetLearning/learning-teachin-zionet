using System.Text.Json.Serialization;

namespace Manager.Models;

public class UserEvent<TPayload>
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EventType eventType { get; set; }
    public TPayload Payload { get; set; } = default!;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventType
{
    ChatAiAnswer,
    TaskUpdate,
}
