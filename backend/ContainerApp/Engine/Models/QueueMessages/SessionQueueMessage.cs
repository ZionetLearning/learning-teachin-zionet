using System.Text.Json;
using System.Text.Json.Serialization;

namespace Engine.Models.QueueMessages;

public record SessionQueueMessage
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageSessionAction ActionName { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FrameKind Frame { get; init; }

    public string SessionId { get; init; } = default!;

    public Guid UserId { get; init; }

    public int Sequence { get; init; }
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