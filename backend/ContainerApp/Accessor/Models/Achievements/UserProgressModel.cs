namespace Accessor.Models.Achievements;

public class UserProgressModel
{
    public Guid UserProgressId { get; set; }
    public required Guid UserId { get; set; }
    public required PracticeFeature Feature { get; set; }
    public int Count { get; set; }
    public DateTime UpdatedAt { get; set; }
}
