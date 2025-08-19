using System.Text.Json.Serialization;

namespace IntegrationTests.Models.Notification;

public class UserNotification
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotificationType Type { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}