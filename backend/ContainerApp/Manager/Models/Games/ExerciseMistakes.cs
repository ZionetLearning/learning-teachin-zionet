using Manager.Models.UserGameConfiguration;

namespace Manager.Models.Games;

public sealed record ExerciseMistakes
{
    public required GameName GameType { get; init; }
    public required Guid ExerciseId { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required List<string> CorrectAnswer { get; init; }
    public required List<MistakeAttempt> Mistakes { get; init; }
}

public sealed record MistakeAttempt
{
    public required Guid AttemptId { get; init; }
    public required List<string> WrongAnswer { get; init; }
    public required decimal Accuracy { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

