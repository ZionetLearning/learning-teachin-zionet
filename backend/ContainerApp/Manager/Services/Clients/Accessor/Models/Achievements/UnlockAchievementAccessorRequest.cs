namespace Manager.Services.Clients.Accessor.Models.Achievements;

public record UnlockAchievementAccessorRequest(
    Guid UserId,
    Guid AchievementId
);
