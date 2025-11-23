using Manager.Models.Achievements;

namespace Manager.Services;

public interface IAchievementService
{
    Task<IReadOnlyList<AchievementDto>> GetUserAchievementsAsync(Guid userId, CancellationToken ct);
    Task<TrackProgressResponse> TrackProgressAsync(TrackProgressRequest request, CancellationToken ct);
}
