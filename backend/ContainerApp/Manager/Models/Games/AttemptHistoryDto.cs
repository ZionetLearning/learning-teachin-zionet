namespace Manager.Models.Games;

public sealed record AttemptHistoryDto
{
    public required Guid ExerciseId { get; init; }
    public required Guid AttemptId { get; init; }
    public required string GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required List<string> GivenAnswer { get; init; }
    public required List<string> CorrectAnswer { get; init; }
    public required AttemptStatus Status { get; init; }
    public required decimal Accuracy { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
