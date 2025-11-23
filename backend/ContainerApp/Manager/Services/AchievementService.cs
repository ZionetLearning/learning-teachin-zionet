using System.Text.Json;
using AutoMapper;
using Manager.Hubs;
using Manager.Models.Achievements;
using Manager.Models.Notifications;
using Manager.Services.Clients.Accessor.Interfaces;
using Manager.Services.Clients.Accessor.Models.Achievements;
using Microsoft.AspNetCore.SignalR;

namespace Manager.Services;

public class AchievementService(
    ILogger<AchievementService> logger,
    IAchievementAccessorClient achievementAccessorClient,
    IHubContext<NotificationHub, INotificationClient> hubContext,
    IMapper mapper
    ) : IAchievementService
{
    private readonly ILogger<AchievementService> _logger = logger;
    private readonly IAchievementAccessorClient _achievementAccessorClient = achievementAccessorClient;
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext = hubContext;
    private readonly IMapper _mapper = mapper;

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

            var result = allAchievements.Select(a =>
            {
                var dto = _mapper.Map<AchievementDto>(a);
                dto.IsUnlocked = unlockedMap.ContainsKey(a.AchievementId);
                dto.UnlockedAt = unlockedMap.TryGetValue(a.AchievementId, out var unlockedAt) ? unlockedAt : null;
                return dto;
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

    public async Task<TrackProgressResponse> TrackProgressAsync(TrackProgressRequest request, CancellationToken ct)
    {
        var sanitizedFeature = request.Feature?.Replace("\r", string.Empty).Replace("\n", string.Empty);
        _logger.LogInformation("Tracking progress for user {UserId}, feature {Feature}, increment {IncrementBy}", 
            request.UserId, sanitizedFeature, request.IncrementBy);

        try
        {
            var progress = await _achievementAccessorClient.GetUserProgressAsync(request.UserId, request.Feature, ct);
            var newCount = (progress?.Count ?? 0) + request.IncrementBy;

            _logger.LogInformation("User {UserId} progress for {Feature}: {OldCount} -> {NewCount}", 
                request.UserId, sanitizedFeature, progress?.Count ?? 0, newCount);

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

            _logger.LogInformation("Found {Count} achievements for feature {Feature}", 
                featureAchievements.Count, sanitizedFeature);

            var unlockedMap = await _achievementAccessorClient.GetUserUnlockedAchievementsAsync(request.UserId, ct);
            var unlockedIds = unlockedMap.Keys.ToHashSet();

            var response = new TrackProgressResponse
            {
                Success = true,
                NewCount = newCount
            };

            foreach (var achievement in featureAchievements)
            {
                if (unlockedIds.Contains(achievement.AchievementId))
                {
                    var sanitizedKey = achievement.Key?.Replace("\r", string.Empty).Replace("\n", string.Empty);
                    _logger.LogDebug("Achievement {Key} already unlocked for user {UserId}", 
                        sanitizedKey, request.UserId);
                    continue;
                }

                if (newCount >= achievement.TargetCount)
                {
                    var sanitizedKey = achievement.Key?.Replace("\r", string.Empty).Replace("\n", string.Empty);
                    _logger.LogInformation("Unlocking achievement {Key} for user {UserId} (count: {Count} >= target: {Target})", 
                        sanitizedKey, request.UserId, newCount, achievement.TargetCount);

                    await _achievementAccessorClient.UnlockAchievementAsync(request.UserId, achievement.AchievementId, ct);

                    response.UnlockedAchievements.Add(achievement.Key);

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

                    _logger.LogInformation("Sent AchievementUnlocked notification for {Key} to user {UserId}", 
                        sanitizedKey, request.UserId);
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking progress for user {UserId}, feature {Feature}", 
                request.UserId, sanitizedFeature);
            throw;
        }
    }
}
