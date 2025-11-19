using Accessor.Models.Achievements;

namespace Accessor.Services.Interfaces;

public interface IAchievementService
{
    Task<List<AchievementModel>> GetAllActiveAchievementsAsync(CancellationToken ct);
    Task<List<AchievementModel>> GetUserUnlockedAchievementsAsync(Guid userId, CancellationToken ct);
    Task<bool> UnlockAchievementAsync(Guid userId, Guid achievementId, CancellationToken ct);
    Task<UserProgressModel?> GetUserProgressAsync(Guid userId, PracticeFeature feature, CancellationToken ct);
    Task UpdateUserProgressAsync(Guid userId, PracticeFeature feature, int count, CancellationToken ct);
}
