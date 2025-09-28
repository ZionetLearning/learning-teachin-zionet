namespace Accessor.Models.Games;

public class GeneratedSentenceDto
{
    public Guid StudentId { get; set; }
    public string GameType { get; set; } = string.Empty;
    public Difficulty Difficulty { get; set; }
    public List<string> CorrectAnswer { get; set; } = new();
}