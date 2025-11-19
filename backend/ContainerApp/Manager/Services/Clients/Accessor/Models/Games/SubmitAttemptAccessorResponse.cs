using Manager.Models.UserGameConfiguration;
using Manager.Models.Games;

namespace Manager.Services.Clients.Accessor.Models.Games;

/// <summary>
/// Response model received from Accessor service after submitting a game attempt
/// </summary>
public sealed record SubmitAttemptAccessorResponse
{
    public required Guid AttemptId { get; init; }
    public required Guid ExerciseId { get; init; }
    public required Guid StudentId { get; init; }
    public required GameName GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required AttemptStatus Status { get; init; }
    public required IReadOnlyList<string> CorrectAnswer { get; init; }
    public required int AttemptNumber { get; init; }
    public required decimal Accuracy { get; init; }
}
