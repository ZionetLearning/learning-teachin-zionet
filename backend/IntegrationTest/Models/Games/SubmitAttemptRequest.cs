namespace IntegrationTests.Models.Games;

public class SubmitAttemptRequest
{
    public required Guid StudentId { get; set; }
    public Guid AttemptId { get; set; }
    public required List<string> GivenAnswer { get; set; } = new();
}
