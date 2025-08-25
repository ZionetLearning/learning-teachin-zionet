using System.Text.Json.Serialization;

namespace Accessor.Models;

public class Notification
{
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotificationType Type { get; set; } = NotificationType.Info;

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}