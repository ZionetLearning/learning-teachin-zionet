namespace Manager.Models.Achievements;

public class AchievementUnlockedNotification
{
    public required Guid AchievementId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}
