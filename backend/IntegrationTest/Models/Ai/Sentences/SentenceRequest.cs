namespace Models.Ai.Sentences;

public sealed class SentenceRequestDto
{
    public Difficulty Difficulty { get; init; } = Difficulty.medium;
    public bool Nikud { get; init; } = false;
    public int Count { get; init; } = 1;
}
public sealed class SentenceRequest
{
    public Difficulty Difficulty { get; init; } = Difficulty.medium;
    public bool Nikud { get; init; } = false;
    public int Count { get; init; } = 1;
    public Guid UserId { get; set; } = Guid.Empty;
}
public enum Difficulty
{
    easy,
    medium,
    hard
}