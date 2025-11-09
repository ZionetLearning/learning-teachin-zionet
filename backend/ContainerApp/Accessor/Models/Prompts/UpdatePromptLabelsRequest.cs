using System.Text.Json.Serialization;

namespace Accessor.Models.Prompts;

public record UpdatePromptLabelsRequest
{
    [JsonPropertyName("newLabels")]
    public required string[] NewLabels { get; set; }
}
