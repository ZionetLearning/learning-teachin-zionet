using Manager.Models.Achievements;
using Manager.Services.Clients.Accessor.Models.Achievements;

namespace Manager.Mapping;

public static class AchievementMapper
{
    public static AchievementDto ToDto(
        this AchievementAccessorModel model,
        bool isUnlocked,
        DateTime? unlockedAt)
    {
        return new AchievementDto
        {
            AchievementId = model.AchievementId,
            Key = model.Key ?? string.Empty,
            Name = model.Name ?? string.Empty,
            Description = model.Description ?? string.Empty,
            Type = model.Type ?? string.Empty,
            Feature = model.Feature ?? string.Empty,
            TargetCount = model.TargetCount,
            IsUnlocked = isUnlocked,
            UnlockedAt = unlockedAt
        };
    }
}
