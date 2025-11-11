using System.Text.Json.Serialization;
using Manager.Models.Games;

namespace Manager.Models.Sentences;

public sealed class SentenceRequestDto
{
    public required string RequestId { get; set; }
    public Difficulty Difficulty { get; init; } = Difficulty.Medium;
    public bool Nikud { get; init; } = false;
    public int Count { get; init; } = 1;
    public GameType GameType { get; init; } = GameType.WordOrderGame;
}

public sealed class SentenceRequest
{
    public required string RequestId { get; set; }
    public Difficulty Difficulty { get; init; } = Difficulty.Medium;
    public bool Nikud { get; init; } = false;
    public int Count { get; init; } = 1;
    public Guid UserId { get; set; } = Guid.Empty;
    public GameType GameType { get; init; } = GameType.WordOrderGame;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Difficulty
{
    Easy,
    Medium,
    Hard
}