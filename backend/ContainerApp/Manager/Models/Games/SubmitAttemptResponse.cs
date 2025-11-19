namespace Manager.Models.Games;

/// <summary>
/// Response model to frontend after submitting a game attempt
/// </summary>
public sealed record SubmitAttemptResponse
{
    public required Guid AttemptId { get; init; }
    public required Guid ExerciseId { get; init; }
    public required Guid StudentId { get; init; }
    public required string GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required AttemptStatus Status { get; init; }
    public required IReadOnlyList<string> CorrectAnswer { get; init; }
    public required int AttemptNumber { get; init; }
    public required decimal Accuracy { get; init; }
}
