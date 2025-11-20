using Manager.Models.Achievements;

namespace Manager.Services.Interfaces;

public interface IAchievementManagerService
{
    Task<IReadOnlyList<AchievementDto>> GetUserAchievementsAsync(Guid userId, CancellationToken ct);
    Task TrackProgressAsync(TrackProgressRequest request, CancellationToken ct);
}
