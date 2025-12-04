using System.Text.Json;
using Manager.Constants;
using Manager.Hubs;
using Manager.Mapping;
using Manager.Models.Achievements;
using Manager.Models.Notifications;
using Manager.Services.Clients.Accessor.Interfaces;
using Manager.Services.Clients.Accessor.Models.Achievements;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Manager.Endpoints;

public static class AchievementEndpoints
{
    private sealed class AchievementEndpoint { }

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static IEndpointRouteBuilder MapAchievementEndpoints(this IEndpointRouteBuilder app)
    {
        var achievementsGroup = app.MapGroup("/achievements-manager").WithTags("Achievements");

        achievementsGroup.MapGet("/user/{userId:guid}", GetUserAchievementsAsync)
            .RequireAuthorization(PolicyNames.AdminOrStudent);

        achievementsGroup.MapPost("/track", TrackProgressAsync)
            .RequireAuthorization(PolicyNames.AdminOrStudent);

        return app;
    }

    private static async Task<IResult> GetUserAchievementsAsync(
        [FromRoute] Guid userId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromServices] ILogger<AchievementEndpoint> log,
        [FromServices] IAchievementAccessorClient achievementAccessorClient,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            log.LogWarning("Invalid userId provided");
            return Results.BadRequest("Invalid userId");
        }

        try
        {
            log.LogInformation("Getting achievements for user {UserId}, FromDate={FromDate}, ToDate={ToDate}",
                userId, fromDate, toDate);

            var allAchievements = await achievementAccessorClient.GetAllActiveAchievementsAsync(fromDate, toDate, ct);
            var unlockedMap = await achievementAccessorClient.GetUserUnlockedAchievementsAsync(userId, fromDate, toDate, ct);

            var result = allAchievements.Select(a => a.ToDto(
                isUnlocked: unlockedMap.ContainsKey(a.AchievementId),
                unlockedAt: unlockedMap.TryGetValue(a.AchievementId, out var unlockedAt) ? unlockedAt : null
            )).ToList();

            log.LogInformation("Returning {Total} achievements ({Unlocked} unlocked) for user {UserId}",
                result.Count, unlockedMap.Count, userId);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to get achievements for user {UserId}", userId);
            return Results.Problem("Failed to get achievements");
        }
    }

    private static async Task<IResult> TrackProgressAsync(
        [FromBody] TrackProgressRequest request,
        [FromServices] ILogger<AchievementEndpoint> log,
        [FromServices] IAchievementAccessorClient achievementAccessorClient,
        [FromServices] IHubContext<NotificationHub, INotificationClient> hubContext,
        CancellationToken ct)
    {
        if (request.UserId == Guid.Empty)
        {
            log.LogWarning("Invalid userId in track progress request");
            return Results.BadRequest("Invalid userId");
        }

        if (string.IsNullOrWhiteSpace(request.Feature))
        {
            log.LogWarning("Missing feature in track progress request");
            return Results.BadRequest("Feature is required");
        }

        if (request.IncrementBy < 1 || request.IncrementBy > 1000)
        {
            log.LogWarning("Invalid IncrementBy value: {IncrementBy}", request.IncrementBy);
            return Results.BadRequest("IncrementBy must be between 1 and 1000");
        }

        try
        {
            var feature = request.Feature ?? throw new ArgumentNullException(nameof(request), "Feature cannot be null");
            var sanitizedFeature = feature.Replace("\r", string.Empty).Replace("\n", string.Empty);

            log.LogInformation("Tracking progress for user {UserId}, feature {Feature}, increment {IncrementBy}",
                request.UserId, sanitizedFeature, request.IncrementBy);

            var progress = await achievementAccessorClient.GetUserProgressAsync(request.UserId, feature, ct);
            var newCount = (progress?.Count ?? 0) + request.IncrementBy;

            log.LogInformation("User {UserId} progress for {Feature}: {OldCount} -> {NewCount}",
                request.UserId, sanitizedFeature, progress?.Count ?? 0, newCount);

            await achievementAccessorClient.UpdateUserProgressAsync(
                request.UserId,
                new UpdateUserProgressAccessorRequest
                {
                    Feature = feature,
                    Count = newCount
                },
                ct);

            var allAchievements = await achievementAccessorClient.GetAllActiveAchievementsAsync(ct: ct);
            var featureAchievements = allAchievements
                .Where(a => a.Feature.Equals(feature, StringComparison.OrdinalIgnoreCase))
                .ToList();

            log.LogInformation("Found {Count} achievements for feature {Feature}",
                featureAchievements.Count, sanitizedFeature);

            var unlockedMap = await achievementAccessorClient.GetUserUnlockedAchievementsAsync(request.UserId, ct: ct);
            var unlockedIds = unlockedMap.Keys.ToHashSet();

            var unlockedAchievements = new List<string>();

            foreach (var achievement in featureAchievements)
            {
                if (unlockedIds.Contains(achievement.AchievementId))
                {
                    var achievementKey = achievement.Key ?? string.Empty;
                    var sanitizedKey = achievementKey.Replace("\r", string.Empty).Replace("\n", string.Empty);
                    log.LogDebug("Achievement {Key} already unlocked for user {UserId}",
                        sanitizedKey, request.UserId);
                    continue;
                }

                if (newCount >= achievement.TargetCount)
                {
                    var achievementKey = achievement.Key ?? string.Empty;
                    var sanitizedKey = achievementKey.Replace("\r", string.Empty).Replace("\n", string.Empty);
                    log.LogInformation("Unlocking achievement {Key} for user {UserId} (count: {Count} >= target: {Target})",
                        sanitizedKey, request.UserId, newCount, achievement.TargetCount);

                    await achievementAccessorClient.UnlockAchievementAsync(request.UserId, achievement.AchievementId, ct);

                    unlockedAchievements.Add(achievementKey);

                    var notification = new AchievementUnlockedNotification
                    {
                        AchievementId = achievement.AchievementId,
                        Key = achievementKey,
                        Name = achievement.Name ?? string.Empty,
                        Description = achievement.Description ?? string.Empty
                    };

                    var jsonPayload = JsonSerializer.SerializeToElement(notification, s_jsonOptions);

                    await hubContext.Clients
                        .User(request.UserId.ToString())
                        .ReceiveEvent(new UserEvent<JsonElement>
                        {
                            EventType = EventType.AchievementUnlocked,
                            Payload = jsonPayload
                        });

                    log.LogInformation("Sent AchievementUnlocked notification for {Key} to user {UserId}",
                        achievementKey, request.UserId);
                }
            }

            var response = new TrackProgressResponse
            {
                Success = true,
                NewCount = newCount,
                UnlockedAchievements = unlockedAchievements
            };

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            var sanitizedFeature = request.Feature?.Replace("\r", string.Empty).Replace("\n", string.Empty);
            log.LogError(ex, "Failed to track progress for user {UserId}, feature {Feature}",
                request.UserId, sanitizedFeature);
            return Results.Problem("Failed to track progress");
        }
    }
}
