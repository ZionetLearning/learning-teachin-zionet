using Manager.Services.Clients.Accessor.Models.Achievements;

namespace Manager.Services.Clients.Accessor.Interfaces;

public interface IAchievementAccessorClient
{
    Task<IReadOnlyList<AchievementAccessorModel>> GetAllActiveAchievementsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<UserAchievementAccessorModel>> GetUserUnlockedAchievementsAsync(Guid userId, CancellationToken ct = default);
    Task UnlockAchievementAsync(Guid userId, Guid achievementId, CancellationToken ct = default);
    Task<UserProgressAccessorModel?> GetUserProgressAsync(Guid userId, string feature, CancellationToken ct = default);
    Task UpdateUserProgressAsync(Guid userId, UpdateUserProgressAccessorRequest request, CancellationToken ct = default);
}
