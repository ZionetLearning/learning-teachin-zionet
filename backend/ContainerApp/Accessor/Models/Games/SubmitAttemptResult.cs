using Accessor.Models.GameConfiguration;

namespace Accessor.Models.Games;

public class SubmitAttemptResult
{
    public required Guid AttemptId { get; set; }
    public required Guid ExerciseId { get; set; }
    public required Guid StudentId { get; set; }
    public required GameName GameType { get; set; }
    public required Difficulty Difficulty { get; set; }
    public required AttemptStatus Status { get; set; }
    public required List<string> CorrectAnswer { get; set; } = new();
    public required int AttemptNumber { get; set; }
}
