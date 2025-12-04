using Accessor.Models.GameConfiguration;

namespace Accessor.Models.Games.Responses;

/// <summary>
/// Response model for getting attempt details
/// </summary>
public sealed record GetAttemptDetailsResponse
{
    public required Guid ExerciseId { get; init; }
    public required Guid AttemptId { get; init; }
    public required GameName GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required List<string> GivenAnswer { get; init; }
    public required List<string> CorrectAnswer { get; init; }
    public required AttemptStatus Status { get; init; }
    public required decimal Accuracy { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

