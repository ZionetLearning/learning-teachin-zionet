namespace Manager.Models.Games;

public class SubmitAttemptRequest
{
    public required Guid StudentId { get; set; }
    public required string GameType { get; set; } = string.Empty;
    public required Difficulty Difficulty { get; set; }
    public required List<string> CorrectAnswer { get; set; } = new();
    public required List<string> GivenAnswer { get; set; } = new();
}
