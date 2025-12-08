using Accessor.Models.GameConfiguration;

namespace Accessor.Models.Games.Responses;

/// <summary>
/// Response model for submitting a game attempt
/// </summary>
public sealed record SubmitAttemptResponse
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

