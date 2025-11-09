namespace Accessor.Models.Games;

public class AttemptedSentenceResult
{
    public Guid ExerciseId { get; set; }

    public string Original { get; set; } = string.Empty;

    public List<string> Words { get; set; } = [];

    public string Difficulty { get; set; } = string.Empty;

    public bool Nikud { get; set; }
}
