using System.Text.Json.Serialization;

namespace Engine.Models.Sentences;

public sealed class SentenceResponse
{
    [JsonPropertyName("sentences")]
    public List<SentenceItem> Sentences { get; init; } = new();
}

public sealed class SentenceItem
{
    [JsonPropertyName("text")]

    public string Text { get; init; } = string.Empty;
    [JsonPropertyName("difficulty")]

    public string Difficulty { get; init; } = string.Empty;
    [JsonPropertyName("nikud")]

    public bool Nikud { get; init; }
}
