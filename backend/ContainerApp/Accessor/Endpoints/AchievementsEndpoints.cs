using Accessor.Models.Achievements;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Endpoints;

public static class AchievementsEndpoints
{
    public static IEndpointRouteBuilder MapAchievementsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/achievements-accessor").WithTags("Achievements");

        group.MapGet("", GetAllAchievementsAsync);
        group.MapGet("/user/{userId:guid}/unlocked", GetUserUnlockedAchievementsAsync);
        group.MapPost("/user/{userId:guid}/unlock/{achievementId:guid}", UnlockAchievementAsync);
        group.MapGet("/user/{userId:guid}/progress/{feature}", GetUserProgressAsync);
        group.MapPut("/user/{userId:guid}/progress", UpdateUserProgressAsync);

        return app;
    }

    private static async Task<IResult> GetAllAchievementsAsync(
        [FromServices] IAchievementService achievementService,
        ILogger<IAchievementService> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("GetAllAchievementsAsync");

        try
        {
            var achievements = await achievementService.GetAllActiveAchievementsAsync(ct);
            logger.LogInformation("Retrieved {Count} active achievements", achievements.Count);
            return Results.Ok(achievements);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while retrieving achievements");
            return Results.StatusCode(499);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error retrieving achievements");
            return Results.Problem("Database error occurred while fetching achievements.", statusCode: 503);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error retrieving achievements");
            return Results.Problem("Unexpected error occurred while fetching achievements.");
        }
    }

    private static async Task<IResult> GetUserUnlockedAchievementsAsync(
        [FromRoute] Guid userId,
        [FromServices] IAchievementService achievementService,
        ILogger<IAchievementService> logger,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            logger.LogWarning("GetUserUnlockedAchievementsAsync called with empty UserId");
            return Results.BadRequest("UserId cannot be empty.");
        }

        using var scope = logger.BeginScope("GetUserUnlockedAchievementsAsync. UserId={UserId}", userId);

        try
        {
            var unlockedAchievements = await achievementService.GetUserUnlockedAchievementsAsync(userId, ct);
            logger.LogInformation("Retrieved {Count} unlocked achievements for user {UserId}", unlockedAchievements.Count, userId);
            return Results.Ok(unlockedAchievements);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while retrieving unlocked achievements for user {UserId}", userId);
            return Results.StatusCode(499);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error retrieving unlocked achievements for user {UserId}", userId);
            return Results.Problem("Database error occurred while fetching unlocked achievements.", statusCode: 503);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error retrieving unlocked achievements for user {UserId}", userId);
            return Results.Problem("Unexpected error occurred while fetching unlocked achievements.");
        }
    }

    private static async Task<IResult> UnlockAchievementAsync(
        [FromRoute] Guid userId,
        [FromRoute] Guid achievementId,
        [FromServices] IAchievementService achievementService,
        ILogger<IAchievementService> logger,
        CancellationToken ct)
    {
        if (userId == Guid.Empty || achievementId == Guid.Empty)
        {
            logger.LogWarning("UnlockAchievementAsync called with empty UserId or AchievementId");
            return Results.BadRequest("UserId and AchievementId cannot be empty.");
        }

        using var scope = logger.BeginScope("UnlockAchievementAsync. UserId={UserId}, AchievementId={AchievementId}", userId, achievementId);

        try
        {
            var success = await achievementService.UnlockAchievementAsync(userId, achievementId, ct);

            if (!success)
            {
                logger.LogWarning("Achievement {AchievementId} already unlocked for user {UserId}", achievementId, userId);
                return Results.Conflict("Achievement already unlocked.");
            }

            logger.LogInformation("Successfully unlocked achievement {AchievementId} for user {UserId}", achievementId, userId);
            return Results.Ok(new { message = "Achievement unlocked successfully" });
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while unlocking achievement {AchievementId} for user {UserId}", achievementId, userId);
            return Results.StatusCode(499);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error unlocking achievement {AchievementId} for user {UserId}. Achievement or user may not exist.", achievementId, userId);
            return Results.Problem("Database error occurred. Achievement or user may not exist.", statusCode: 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error unlocking achievement {AchievementId} for user {UserId}", achievementId, userId);
            return Results.Problem("Unexpected error occurred while unlocking achievement.");
        }
    }

    private static async Task<IResult> GetUserProgressAsync(
        [FromRoute] Guid userId,
        [FromRoute] string feature,
        [FromServices] IAchievementService achievementService,
        ILogger<IAchievementService> logger,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            logger.LogWarning("GetUserProgressAsync called with empty UserId");
            return Results.BadRequest("UserId cannot be empty.");
        }

        if (!Enum.TryParse<PracticeFeature>(feature, true, out var practiceFeature))
        {
            logger.LogWarning("GetUserProgressAsync called with invalid feature: {Feature}", feature);
            return Results.BadRequest($"Invalid feature: {feature}");
        }

        using var scope = logger.BeginScope("GetUserProgressAsync. UserId={UserId}, Feature={Feature}", userId, practiceFeature);

        try
        {
            var progress = await achievementService.GetUserProgressAsync(userId, practiceFeature, ct);

            if (progress == null)
            {
                logger.LogInformation("No progress found for user {UserId}, feature {Feature}", userId, practiceFeature);
                return Results.Ok(new { userId, feature = practiceFeature, count = 0 });
            }

            logger.LogInformation("Retrieved progress for user {UserId}, feature {Feature}: count={Count}", userId, practiceFeature, progress.Count);
            return Results.Ok(progress);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while retrieving progress for user {UserId}, feature {Feature}", userId, practiceFeature);
            return Results.StatusCode(499);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error retrieving progress for user {UserId}, feature {Feature}", userId, practiceFeature);
            return Results.Problem("Database error occurred while fetching progress.", statusCode: 503);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error retrieving progress for user {UserId}, feature {Feature}", userId, practiceFeature);
            return Results.Problem("Unexpected error occurred while fetching progress.");
        }
    }

    private static async Task<IResult> UpdateUserProgressAsync(
        [FromRoute] Guid userId,
        [FromBody] UpdateProgressRequest request,
        [FromServices] IAchievementService achievementService,
        ILogger<IAchievementService> logger,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            logger.LogWarning("UpdateUserProgressAsync called with empty UserId");
            return Results.BadRequest("UserId cannot be empty.");
        }

        if (request == null)
        {
            logger.LogWarning("UpdateUserProgressAsync called with null request");
            return Results.BadRequest("Request body cannot be null.");
        }

        using var scope = logger.BeginScope("UpdateUserProgressAsync. UserId={UserId}, Feature={Feature}, Count={Count}", userId, request.Feature, request.Count);

        try
        {
            var success = await achievementService.UpdateUserProgressAsync(userId, request.Feature, request.Count, ct);

            if (!success)
            {
                logger.LogWarning("Failed to update progress for user {UserId}, feature {Feature}", userId, request.Feature);
                return Results.Problem("Failed to update progress.");
            }

            logger.LogInformation("Successfully updated progress for user {UserId}, feature {Feature} to count {Count}", userId, request.Feature, request.Count);
            return Results.Ok(new { message = "Progress updated successfully" });
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while updating progress for user {UserId}, feature {Feature}", userId, request.Feature);
            return Results.StatusCode(499);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating progress for user {UserId}, feature {Feature}. Possible constraint violation.", userId, request.Feature);
            return Results.Problem("Database error occurred. User may not exist or constraint violation.", statusCode: 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error updating progress for user {UserId}, feature {Feature}", userId, request.Feature);
            return Results.Problem("Unexpected error occurred while updating progress.");
        }
    }
}

public record UpdateProgressRequest(
    PracticeFeature Feature,
    int Count
);
