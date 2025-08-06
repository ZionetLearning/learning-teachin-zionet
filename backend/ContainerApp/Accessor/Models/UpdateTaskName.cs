using System.Text.Json.Serialization;

namespace Accessor.Models;

public record UpdateTaskName
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}
