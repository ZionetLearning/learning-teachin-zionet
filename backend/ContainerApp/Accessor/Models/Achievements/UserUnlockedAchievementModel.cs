namespace Accessor.Models.Achievements;

public class UserUnlockedAchievementModel
{
    // Achievement data
    public required Guid AchievementId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required AchievementType Type { get; set; }
    public required PracticeFeature Feature { get; set; }
    public required int TargetCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    // UserAchievement data
    public required Guid UserAchievementId { get; set; }
    public required Guid UserId { get; set; }
    public DateTime UnlockedAt { get; set; }
}
