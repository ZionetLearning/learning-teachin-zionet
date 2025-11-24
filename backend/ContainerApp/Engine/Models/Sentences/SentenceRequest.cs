using System.Text.Json.Serialization;

namespace Engine.Models.Sentences;

public sealed class SentenceRequest
{
    public required string RequestId { get; init; }
    public Difficulty Difficulty { get; init; } = Difficulty.Medium;
    public bool Nikud { get; init; } = false;
    public int Count { get; init; } = 1;
    public required Guid UserId { get; init; }
    public GameType GameType { get; init; } = GameType.WordOrderGame;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Difficulty
{
    Easy,
    Medium,
    Hard
}
