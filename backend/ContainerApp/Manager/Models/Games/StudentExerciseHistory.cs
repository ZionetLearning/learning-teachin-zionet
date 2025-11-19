using Manager.Models.UserGameConfiguration;

namespace Manager.Models.Games;

public sealed record StudentExerciseHistory
{
    public required Guid StudentId { get; init; }
    public required GameName GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required int AttemptsCount { get; init; }
    public required int TotalSuccesses { get; init; }
    public required int TotalFailures { get; init; }
    public required string StudentFirstName { get; init; }
    public required string StudentLastName { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}