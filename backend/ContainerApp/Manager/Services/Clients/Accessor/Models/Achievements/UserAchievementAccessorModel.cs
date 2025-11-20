namespace Manager.Services.Clients.Accessor.Models.Achievements;

public class UserAchievementAccessorModel
{
    public required Guid UserAchievementId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid AchievementId { get; set; }
    public DateTime UnlockedAt { get; set; }
}
