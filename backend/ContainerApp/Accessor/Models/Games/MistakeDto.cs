using Accessor.Models.GameConfiguration;

namespace Accessor.Models.Games;

public class MistakeDto
{
    public required Guid AttemptId { get; set; }
    public required GameName GameType { get; set; }
    public required Difficulty Difficulty { get; set; }
    public List<string> CorrectAnswer { get; set; } = new();
    public List<List<string>> WrongAnswers { get; set; } = new();
}