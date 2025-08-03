using System.Text.Json.Serialization;

namespace Engine.Models;

public class TaskModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("payload")]
    public required string Payload { get; set; }
}
