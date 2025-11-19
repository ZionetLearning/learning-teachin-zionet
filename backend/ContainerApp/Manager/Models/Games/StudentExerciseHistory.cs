using Manager.Models.UserGameConfiguration;

namespace Manager.Models.Games;

public class StudentExerciseHistory
{
    public required Guid StudentId { get; set; }
    public required GameName GameType { get; set; }
    public required Difficulty Difficulty { get; set; }
    public required int AttemptsCount { get; set; }
    public required int TotalSuccesses { get; set; }
    public required int TotalFailures { get; set; }
    public required string StudentFirstName { get; set; }
    public required string StudentLastName { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
}