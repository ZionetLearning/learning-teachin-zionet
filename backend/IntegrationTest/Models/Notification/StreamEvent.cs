using System.Text.Json.Serialization;

namespace IntegrationTests.Models.Notification;

public class StreamEvent<TPayload>
{
    [JsonPropertyName("eventType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StreamEventType EventType { get; set; }

    [JsonPropertyName("payload")]
    public TPayload Payload { get; set; } = default!;

    [JsonPropertyName("sequenceNumber")]
    public int SequenceNumber { get; set; }

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;

    [JsonPropertyName("stage")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StreamEventStage Stage { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StreamEventType
{
    Unknown = 0,
    ChatAiAnswer
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StreamEventStage
{
    Unknown = 0,
    First,
    Chunk,
    Last,
    Heartbeat,
    Error,
}
