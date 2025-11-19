namespace Manager.Services.Clients.Accessor.Models.Games;

/// <summary>
/// Request model sent to Accessor service for submitting a game attempt
/// </summary>
public class SubmitAttemptAccessorRequest
{
    public required Guid StudentId { get; set; }
    public Guid ExerciseId { get; set; }
    public required List<string> GivenAnswer { get; set; } = new();
}
