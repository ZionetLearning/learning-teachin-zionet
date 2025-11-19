using Manager.Models.UserGameConfiguration;

namespace Manager.Models.Games;

public class ExerciseMistakes
{
    public required GameName GameType { get; set; }
    public required Guid ExerciseId { get; set; }
    public required Difficulty Difficulty { get; set; }
    public required List<string> CorrectAnswer { get; set; } = new();
    public required List<MistakeAttempt> Mistakes { get; set; } = new();
}

public class MistakeAttempt
{
    public required Guid AttemptId { get; set; }
    public required List<string> WrongAnswer { get; set; } = new();
    public required decimal Accuracy { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
}

