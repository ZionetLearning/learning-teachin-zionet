using Accessor.Models.GameConfiguration;

namespace Accessor.Models.Games;

public class GeneratedSentenceDto
{
    public Guid StudentId { get; set; }
    public GameName GameType { get; set; }
    public Difficulty Difficulty { get; set; }
    public List<GeneratedSentenceItem> Sentences { get; set; } = [];
}

public class GeneratedSentenceItem
{
    public string Original { get; set; } = string.Empty;
    public List<string> CorrectAnswer { get; set; } = [];
    public bool Nikud { get; set; }
}
