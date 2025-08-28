using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Manager.Models.Notifications;

public class UserNotification
{
    [Required(ErrorMessage = "Message is required.")]
    [MaxLength(400, ErrorMessage = "Message cannot exceed 400 characters.")]
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [Required(ErrorMessage = "Type is required.")]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotificationType Type { get; set; }

    [Required(ErrorMessage = "Timestamp is required.")]
    [JsonPropertyName("timestamp")]
    [DataType(DataType.DateTime, ErrorMessage = "Timestamp must be a valid date and time.")]
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