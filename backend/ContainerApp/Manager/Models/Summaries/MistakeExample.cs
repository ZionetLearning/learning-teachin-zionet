namespace Manager.Models.Summaries;

public sealed record MistakeExample
{
    public required Guid ExerciseId { get; init; }
    public required string GameType { get; init; }
    public required string Difficulty { get; init; }
    public List<string> CorrectAnswer { get; init; } = new();
    public List<string> GivenAnswer { get; init; } = new();
    public decimal Accuracy { get; init; }
    public int AttemptNumber { get; init; }
    public required DateTime CreatedAt { get; init; }
}
