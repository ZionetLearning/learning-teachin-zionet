using System.Text.Json.Serialization;

namespace Manager.Models.Notifications;

public class UserEvent<TPayload>
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EventType EventType { get; set; }
    public TPayload Payload { get; set; } = default!;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventType
{
    ChatAiAnswer,
    TaskUpdate,
}
