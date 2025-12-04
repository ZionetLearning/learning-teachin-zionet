namespace Manager.Models.Summaries;

public sealed record MistakePattern
{
    public required string GameType { get; init; }
    public required string Difficulty { get; init; }
    public int Count { get; init; }
}
