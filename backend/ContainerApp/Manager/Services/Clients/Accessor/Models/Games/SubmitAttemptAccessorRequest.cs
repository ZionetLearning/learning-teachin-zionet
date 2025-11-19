namespace Manager.Services.Clients.Accessor.Models.Games;

/// <summary>
/// Request model sent to Accessor service for submitting a game attempt
/// </summary>
public sealed record SubmitAttemptAccessorRequest
{
    public required Guid StudentId { get; init; }
    public Guid ExerciseId { get; init; }
    public required IReadOnlyList<string> GivenAnswer { get; init; }
}
