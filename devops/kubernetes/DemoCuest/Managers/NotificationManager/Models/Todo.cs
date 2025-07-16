using System.Text.Json.Serialization;

namespace NotificationManager.Models;

public class Todo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
