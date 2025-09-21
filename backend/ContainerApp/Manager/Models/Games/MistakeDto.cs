namespace Manager.Models.Games;

public class MistakeDto
{
    public required string GameType { get; set; } = string.Empty;
    public required Difficulty Difficulty { get; set; }
    public List<string> CorrectAnswer { get; set; } = new();
    public List<List<string>> WrongAnswers { get; set; } = new();
}