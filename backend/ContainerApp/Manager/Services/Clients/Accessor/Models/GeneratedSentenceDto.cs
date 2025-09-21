using Manager.Models.Games;

namespace Manager.Services.Clients.Accessor.Models;

public class GeneratedSentenceDto
{
    public Guid StudentId { get; set; }
    public string GameType { get; set; } = string.Empty;
    public Difficulty Difficulty { get; set; }
    public List<string> CorrectAnswer { get; set; } = new();
}