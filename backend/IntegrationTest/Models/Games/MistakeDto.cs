namespace IntegrationTests.Models.Games;

public class MistakeDto
{
    public required Guid AttemptId { get; set; }
    public required string GameType { get; set; } = string.Empty;
    public required string Difficulty { get; set; }
    public List<string> CorrectAnswer { get; set; } = new();
    public List<List<string>> WrongAnswers { get; set; } = new();
}
