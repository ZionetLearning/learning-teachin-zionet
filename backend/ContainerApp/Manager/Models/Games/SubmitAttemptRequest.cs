namespace Manager.Models.Games;

public class SubmitAttemptRequest
{
    public Guid ExerciseId { get; set; }
    public required List<string> GivenAnswer { get; set; } = new();
}
