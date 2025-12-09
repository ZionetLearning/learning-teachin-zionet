namespace Manager.Services.Clients.Accessor.Models.Games;

/// <summary>
/// Request model sent to Accessor service for submitting a game attempt
/// Matches Accessor's SubmitAttemptRequest exactly
/// </summary>
public sealed record SubmitAttemptAccessorRequest
{
    public required Guid StudentId { get; init; }
    public required Guid ExerciseId { get; init; }
    public required List<string> GivenAnswer { get; init; }
}
