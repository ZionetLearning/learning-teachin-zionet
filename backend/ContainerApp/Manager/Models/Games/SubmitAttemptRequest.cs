namespace Manager.Models.Games;

/// <summary>
/// Request model from frontend for submitting a game attempt
/// </summary>
public sealed class SubmitAttemptRequest
{
    public Guid ExerciseId { get; set; }
    public required List<string> GivenAnswer { get; set; } = new();
}
