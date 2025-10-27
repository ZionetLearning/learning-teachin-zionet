using System.Text.Json.Serialization;

namespace Engine.Models.Sentences;

public sealed class SentenceRequest
{
    public Difficulty Difficulty { get; init; } = Difficulty.Medium;
    public bool Nikud { get; init; } = false;
    public int Count { get; init; } = 1;
    public required Guid UserId { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Difficulty
{
    Easy,
    Medium,
    Hard
}
