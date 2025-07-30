using System.Text.Json.Serialization;

namespace IntegrationTests.Models;

public class TaskModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;
}
