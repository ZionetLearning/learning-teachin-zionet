using Accessor.Models.Achievements;

namespace Accessor.Services.Interfaces;

public interface IAchievementService
{
    Task<IReadOnlyList<AchievementModel>> GetAllActiveAchievementsAsync(CancellationToken ct);
    Task<IReadOnlyList<AchievementModel>> GetUserUnlockedAchievementsAsync(Guid userId, CancellationToken ct);
    Task<bool> UnlockAchievementAsync(Guid userId, Guid achievementId, CancellationToken ct);
    Task UpsertUserProgressAsync(Guid userId, PracticeFeature feature, int count, CancellationToken ct);
    Task<IReadOnlyList<AchievementModel>> GetEligibleAchievementsAsync(PracticeFeature feature, int count, CancellationToken ct);
}
