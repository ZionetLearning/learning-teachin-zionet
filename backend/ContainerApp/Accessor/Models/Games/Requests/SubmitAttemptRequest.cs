namespace Accessor.Models.Games.Requests;

/// <summary>
/// Request model for submitting a game attempt
/// </summary>
public sealed record SubmitAttemptRequest
{
    public required Guid StudentId { get; init; }
    public required Guid ExerciseId { get; init; }
    public required List<string> GivenAnswer { get; init; }
}

