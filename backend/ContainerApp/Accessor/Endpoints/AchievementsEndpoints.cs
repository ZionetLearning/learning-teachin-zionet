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
        group.MapPost("/unlock", UnlockAchievementAsync);
        group.MapPut("/user/{userId:guid}/progress", UpdateUserProgressAsync);

        return app;
    }

    private static async Task<IResult> GetAllAchievementsAsync(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromServices] IAchievementService achievementService,
        ILogger<IAchievementService> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope(
            "GetAllAchievementsAsync. FromDate={FromDate}, ToDate={ToDate}",
            fromDate, toDate);

        try
        {
            var achievements = await achievementService.GetAllActiveAchievementsAsync(fromDate, toDate, ct);
            logger.LogInformation("Retrieved {Count} active achievements", achievements.Count);
            return Results.Ok(achievements);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while retrieving achievements");
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving achievements");
            return Results.Problem("Error occurred while fetching achievements.", statusCode: 500);
        }
    }

    private static async Task<IResult> GetUserUnlockedAchievementsAsync(
        [FromRoute] Guid userId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromServices] IAchievementService achievementService,
        ILogger<IAchievementService> logger,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            logger.LogWarning("GetUserUnlockedAchievementsAsync called with empty UserId");
            return Results.BadRequest("UserId cannot be empty.");
        }

        using var scope = logger.BeginScope("GetUserUnlockedAchievementsAsync. UserId={UserId}, FromDate={FromDate}, ToDate={ToDate}", userId, fromDate, toDate);

        try
        {
            var unlockedAchievements = await achievementService.GetUserUnlockedAchievementsAsync(userId, fromDate, toDate, ct);
            logger.LogInformation("Retrieved {Count} unlocked achievements for user {UserId}", unlockedAchievements.Count, userId);
            return Results.Ok(unlockedAchievements);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while retrieving unlocked achievements for user {UserId}", userId);
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving unlocked achievements for user {UserId}", userId);
            return Results.Problem("Error occurred while fetching unlocked achievements.", statusCode: 500);
        }
    }

    private static async Task<IResult> UnlockAchievementAsync(
        [FromBody] UnlockAchievementRequest request,
        [FromServices] IAchievementService achievementService,
        ILogger<IAchievementService> logger,
        CancellationToken ct)
    {
        if (request == null)
        {
            logger.LogWarning("UnlockAchievementAsync called with null request");
            return Results.BadRequest("Request body cannot be null.");
        }

        if (request.UserId == Guid.Empty || request.AchievementId == Guid.Empty)
        {
            logger.LogWarning("UnlockAchievementAsync called with empty UserId or AchievementId");
            return Results.BadRequest("UserId and AchievementId cannot be empty.");
        }

        using var scope = logger.BeginScope("UnlockAchievementAsync. UserId={UserId}, AchievementId={AchievementId}", request.UserId, request.AchievementId);

        try
        {
            var success = await achievementService.UnlockAchievementAsync(request.UserId, request.AchievementId, ct);

            if (!success)
            {
                logger.LogWarning("Achievement {AchievementId} already unlocked for user {UserId}", request.AchievementId, request.UserId);
                return Results.Conflict("Achievement already unlocked.");
            }

            logger.LogInformation("Successfully unlocked achievement {AchievementId} for user {UserId}", request.AchievementId, request.UserId);
            return Results.Ok(new { message = "Achievement unlocked successfully" });
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while unlocking achievement {AchievementId} for user {UserId}", request.AchievementId, request.UserId);
            return Results.StatusCode(499);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error unlocking achievement {AchievementId} for user {UserId}. Achievement or user may not exist.", request.AchievementId, request.UserId);
            return Results.Problem("Database error occurred. Achievement or user may not exist.", statusCode: 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error unlocking achievement {AchievementId} for user {UserId}", request.AchievementId, request.UserId);
            return Results.Problem("Unexpected error occurred while unlocking achievement.");
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

        if (request.Count < 0)
        {
            logger.LogWarning("UpdateUserProgressAsync called with negative count: {Count}", request.Count);
            return Results.BadRequest("Count cannot be negative.");
        }

        using var scope = logger.BeginScope("UpdateUserProgressAsync. UserId={UserId}, Feature={Feature}, Count={Count}", userId, request.Feature, request.Count);

        try
        {
            await achievementService.UpsertUserProgressAsync(userId, request.Feature, request.Count, ct);

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
