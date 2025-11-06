namespace Manager.Models.Games;

public class MistakeDto
{
    public required Guid ExerciseId { get; set; }
    public required string GameType { get; set; } = string.Empty;
    public required Difficulty Difficulty { get; set; }
    public required List<string> CorrectAnswer { get; set; } = new();
    public required List<MistakeAttemptDto> Mistakes { get; set; } = new();
}

public class MistakeAttemptDto
{
    public required Guid AttemptId { get; set; }
    public required List<string> WrongAnswer { get; set; } = new();
    public required decimal Accuracy { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
}

