namespace Accessor.Models.Achievements;

public class UserAchievementModel
{
    public Guid UserAchievementId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid AchievementId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UnlockedAt { get; set; }
}
