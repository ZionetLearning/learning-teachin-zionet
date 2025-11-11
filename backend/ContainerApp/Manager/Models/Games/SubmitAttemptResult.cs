using Manager.Models.UserGameConfiguration;

namespace Manager.Models.Games;

public class SubmitAttemptResult
{
    public required Guid StudentId { get; set; }
    public required GameName GameType { get; set; }
    public required Difficulty Difficulty { get; set; }
    public required AttemptStatus Status { get; set; }
    public required List<string> CorrectAnswer { get; set; } = new();
    public required int AttemptNumber { get; set; }
}