namespace Manager.Services.Clients.Accessor.Models;

public class SubmitAttemptRequestDto
{
    public required Guid StudentId { get; set; }
    public Guid ExerciseId { get; set; }
    public required List<string> GivenAnswer { get; set; } = new();
}
