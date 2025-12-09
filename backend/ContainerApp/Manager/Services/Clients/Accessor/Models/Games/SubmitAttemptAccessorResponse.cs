using Manager.Models.Games;
using Manager.Models.UserGameConfiguration;

namespace Manager.Services.Clients.Accessor.Models.Games;

/// <summary>
/// Response model received from Accessor service after submitting a game attempt
/// Matches Accessor's SubmitAttemptResponse exactly
/// </summary>
public sealed record SubmitAttemptAccessorResponse
{
    public required Guid AttemptId { get; init; }
    public required Guid ExerciseId { get; init; }
    public required Guid StudentId { get; init; }
    public required GameName GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required AttemptStatus Status { get; init; }
    public required List<string> CorrectAnswer { get; init; }
    public required int AttemptNumber { get; init; }
    public required decimal Accuracy { get; init; }
}
