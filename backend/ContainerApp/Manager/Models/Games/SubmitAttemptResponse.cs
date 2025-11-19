namespace Manager.Models.Games;

/// <summary>
/// Response model to frontend after submitting a game attempt
/// </summary>
public sealed class SubmitAttemptResponse
{
    public required Guid AttemptId { get; set; }
    public required Guid ExerciseId { get; set; }
    public required Guid StudentId { get; set; }
    public required string GameType { get; set; }
    public required Difficulty Difficulty { get; set; }
    public required AttemptStatus Status { get; set; }
    public required List<string> CorrectAnswer { get; set; } = new();
    public required int AttemptNumber { get; set; }
    public required decimal Accuracy { get; set; }
}
