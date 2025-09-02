namespace Engine.Models.Sentences;

public sealed class SentenceRequest
{
    public Difficulty Difficulty { get; init; } = Difficulty.medium;
    public bool Nikud { get; init; } = false;
    public int Count { get; init; } = 1;
    public required Guid UserId { get; init; }
}
public enum Difficulty
{
    easy,
    medium,
    hard
}