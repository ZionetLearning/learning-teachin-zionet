using System.Text.Json.Serialization;

namespace Manager.Models.Notifications;

public class StreamEvent<TPayload>
{
    public StreamEventType EventType { get; set; }

    public TPayload Payload { get; set; } = default!;

    public int SequenceNumber { get; set; }

    public StreamEventStage Stage { get; set; }

    public string RequestId { get; set; } = default!;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StreamEventType
{
    ChatAiAnswer,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StreamEventStage
{
    First,
    Chunk,
    Last,
    Heartbeat,
    Error
}
