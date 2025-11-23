using System.Text.Json;
using Manager.Hubs;
using Manager.Models.Achievements;
using Manager.Models.Notifications;
using Manager.Services.Clients.Accessor.Interfaces;
using Manager.Services.Clients.Accessor.Models.Achievements;
using Manager.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Manager.Services;

public class AchievementManagerService(
    ILogger<AchievementManagerService> logger,
    IAchievementAccessorClient achievementAccessorClient,
    IHubContext<NotificationHub, INotificationClient> hubContext
    ) : IAchievementManagerService
{
    private readonly ILogger<AchievementManagerService> _logger = logger;
    private readonly IAchievementAccessorClient _achievementAccessorClient = achievementAccessorClient;
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext = hubContext;

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<IReadOnlyList<AchievementDto>> GetUserAchievementsAsync(Guid userId, CancellationToken ct)
    {
        _logger.LogInformation("Getting achievements for user {UserId}", userId);

        try
        {
            var allAchievements = await _achievementAccessorClient.GetAllActiveAchievementsAsync(ct);
            var unlockedMap = await _achievementAccessorClient.GetUserUnlockedAchievementsAsync(userId, ct);

            var result = allAchievements.Select(a => new AchievementDto
            {
                AchievementId = a.AchievementId,
                Key = a.Key,
                Name = a.Name,
                Description = a.Description,
                Type = a.Type,
                Feature = a.Feature,
                TargetCount = a.TargetCount,
                IsUnlocked = unlockedMap.ContainsKey(a.AchievementId),
                UnlockedAt = unlockedMap.TryGetValue(a.AchievementId, out var unlockedAt) ? unlockedAt : null
            }).ToList();

            _logger.LogInformation("Returning {Total} achievements ({Unlocked} unlocked) for user {UserId}", result.Count, unlockedMap.Count, userId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting achievements for user {UserId}", userId);
            throw;
        }
    }

    public async Task TrackProgressAsync(TrackProgressRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Tracking progress for user {UserId}, feature {Feature}, increment {IncrementBy}", request.UserId, request.Feature, request.IncrementBy);

        try
        {
            var progress = await _achievementAccessorClient.GetUserProgressAsync(request.UserId, request.Feature, ct);
            var newCount = (progress?.Count ?? 0) + request.IncrementBy;

            _logger.LogInformation("User {UserId} progress for {Feature}: {OldCount} -> {NewCount}", request.UserId, request.Feature, progress?.Count ?? 0, newCount);

            await _achievementAccessorClient.UpdateUserProgressAsync(
                request.UserId,
                new UpdateUserProgressAccessorRequest
                {
                    Feature = request.Feature,
                    Count = newCount
                },
                ct);

            var allAchievements = await _achievementAccessorClient.GetAllActiveAchievementsAsync(ct);
            var featureAchievements = allAchievements
                .Where(a => a.Feature.Equals(request.Feature, StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger.LogInformation("Found {Count} achievements for feature {Feature}", featureAchievements.Count, request.Feature);

            var unlockedMap = await _achievementAccessorClient.GetUserUnlockedAchievementsAsync(request.UserId, ct);
            var unlockedIds = unlockedMap.Keys.ToHashSet();

            foreach (var achievement in featureAchievements)
            {
                if (unlockedIds.Contains(achievement.AchievementId))
                {
                    _logger.LogDebug("Achievement {Key} already unlocked for user {UserId}", achievement.Key, request.UserId);
                    continue;
                }

                if (newCount >= achievement.TargetCount)
                {
                    _logger.LogInformation("Unlocking achievement {Key} for user {UserId} (count: {Count} >= target: {Target})", achievement.Key, request.UserId, newCount, achievement.TargetCount);

                    await _achievementAccessorClient.UnlockAchievementAsync(request.UserId, achievement.AchievementId, ct);

                    var notification = new AchievementUnlockedNotification
                    {
                        AchievementId = achievement.AchievementId,
                        Key = achievement.Key,
                        Name = achievement.Name,
                        Description = achievement.Description
                    };

                    var jsonPayload = JsonSerializer.SerializeToElement(notification, s_jsonOptions);

                    await _hubContext.Clients
                        .User(request.UserId.ToString())
                        .ReceiveEvent(new UserEvent<JsonElement>
                        {
                            EventType = EventType.AchievementUnlocked,
                            Payload = jsonPayload
                        });

                    _logger.LogInformation("Sent AchievementUnlocked notification for {Key} to user {UserId}", achievement.Key, request.UserId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking progress for user {UserId}, feature {Feature}", request.UserId, request.Feature);
            throw;
        }
    }
}
