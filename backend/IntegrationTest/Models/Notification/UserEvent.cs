using System.Text.Json.Serialization;

namespace IntegrationTests.Models.Notification;

public class UserEvent<T>
{
    [JsonPropertyName("eventType")]
    public EventType EventType { get; set; }

    [JsonPropertyName("payload")]
    public T Payload { get; set; } = default!;
}

public enum EventType
{
    ChatAiAnswer,
    TaskUpdate,
}