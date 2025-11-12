using Accessor.Models.GameConfiguration;

namespace Accessor.Models.Games;

public class AttemptHistoryDto
{
    public required Guid ExerciseId { get; set; }
    public required Guid AttemptId { get; set; }
    public required GameName GameType { get; set; }
    public required Difficulty Difficulty { get; set; }
    public required List<string> GivenAnswer { get; set; } = new();
    public required List<string> CorrectAnswer { get; set; } = new();
    public required AttemptStatus Status { get; set; }
    public required decimal Accuracy { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
}
