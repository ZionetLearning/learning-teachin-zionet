using Accessor.DB;
using Accessor.Models.Achievements;
using Accessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class AchievementService : IAchievementService
{
    private readonly AccessorDbContext _context;
    private readonly ILogger<AchievementService> _logger;

    public AchievementService(AccessorDbContext context, ILogger<AchievementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AchievementModel>> GetAllActiveAchievementsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        try
        {
            var query = _context.Achievements
                .Where(a => a.IsActive);

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= toDate.Value);
            }

            return await query
                .OrderBy(a => a.Feature)
                .ThenBy(a => a.TargetCount)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all active achievements");
            throw;
        }
    }

    public async Task<IReadOnlyList<AchievementModel>> GetUserUnlockedAchievementsAsync(Guid userId, DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        try
        {
            var unlockedQuery = _context.UserAchievements
                .Where(ua => ua.UserId == userId);

            if (fromDate.HasValue)
            {
                unlockedQuery = unlockedQuery.Where(ua => ua.CreatedAt >= fromDate.Value || ua.UnlockedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                unlockedQuery = unlockedQuery.Where(ua => ua.CreatedAt <= toDate.Value || ua.UnlockedAt <= toDate.Value);
            }

            var unlockedAchievementIds = await unlockedQuery
                .Select(ua => ua.AchievementId)
                .ToListAsync(ct);

            return await _context.Achievements
                .Where(a => unlockedAchievementIds.Contains(a.AchievementId))
                .OrderBy(a => a.Feature)
                .ThenBy(a => a.TargetCount)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unlocked achievements for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UnlockAchievementAsync(Guid userId, Guid achievementId, CancellationToken ct)
    {
        try
        {
            var existingUnlock = await _context.UserAchievements
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId, ct);

            if (existingUnlock != null)
            {
                return false;
            }

            var userAchievement = new UserAchievementModel
            {
                UserAchievementId = Guid.NewGuid(),
                UserId = userId,
                AchievementId = achievementId,
                UnlockedAt = DateTime.UtcNow
            };

            _context.UserAchievements.Add(userAchievement);
            await _context.SaveChangesAsync(ct);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking achievement {AchievementId} for user {UserId}", achievementId, userId);
            throw;
        }
    }

    public async Task UpsertUserProgressAsync(Guid userId, PracticeFeature feature, int count, CancellationToken ct)
    {
        try
        {
            var progress = await _context.UserProgress
                .FirstOrDefaultAsync(up => up.UserId == userId && up.Feature == feature, ct);

            if (progress == null)
            {
                progress = new UserProgressModel
                {
                    UserProgressId = Guid.NewGuid(),
                    UserId = userId,
                    Feature = feature,
                    Count = count,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserProgress.Add(progress);
            }
            else
            {
                progress.Count = count;
                progress.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress for user {UserId}, feature {Feature}, count {Count}", userId, feature, count);
            throw;
        }
    }

    public async Task<IReadOnlyList<AchievementModel>> GetEligibleAchievementsAsync(PracticeFeature feature, int count, CancellationToken ct)
    {
        try
        {
            return await _context.Achievements
                .Where(a => a.IsActive && a.Feature == feature && a.TargetCount <= count)
                .OrderBy(a => a.TargetCount)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving eligible achievements for feature {Feature}, count {Count}", feature, count);
            throw;
        }
    }
}
