using System.Text.Json;
using System.Text.Json.Serialization;

namespace Manager.Models.QueueMessages;

public record SessionQueueMessage
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageSessionAction ActionName { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FrameKind Frame { get; init; }

    public string SessionId { get; init; } = default!;

    public Guid UserId { get; init; }

    public int Sequence { get; init; }
    // Request id (or other id if it is not coming from request) correlation ID to trace the entire request flow
    public required string CorrelationId { get; init; }

    public JsonElement Payload { get; init; }

    public JsonElement? Metadata { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public int? TtlSeconds { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageSessionAction
{
    ChatStream
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FrameKind
{
    First,
    Chunk,
    Last,
    Heartbeat,
    Error
}