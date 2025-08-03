using System.Text.Json.Serialization;

namespace Engine.Models
{
    public record TaskModel
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }
        [JsonPropertyName("name")]
        public required string Name { get; init; }
        [JsonPropertyName("payload")]
        public required string Payload { get; init; }
    }
}
