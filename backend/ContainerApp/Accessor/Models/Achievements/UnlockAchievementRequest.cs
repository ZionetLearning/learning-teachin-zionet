namespace Accessor.Models.Achievements;

public record UnlockAchievementRequest(
    Guid UserId,
    Guid AchievementId
);
