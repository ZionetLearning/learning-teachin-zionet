using System.Text.Json.Serialization;

namespace Manager.Models.Sentences;

public sealed class SentenceRequestDto
{
    public Difficulty Difficulty { get; init; } = Difficulty.Medium;
    public bool Nikud { get; init; } = false;
    public int Count { get; init; } = 1;
}
public sealed class SentenceRequest
{
    public Difficulty Difficulty { get; init; } = Difficulty.Medium;
    public bool Nikud { get; init; } = false;
    public int Count { get; init; } = 1;
    public Guid UserId { get; set; } = Guid.Empty;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Difficulty
{
    Easy,
    Medium,
    Hard
}