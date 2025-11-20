using Manager.Models.Achievements;
using Manager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class AchievementManagerEndpoints
{
    private sealed class AchievementManagerEndpoint { }

    public static IEndpointRouteBuilder MapAchievementManager(this IEndpointRouteBuilder app)
    {
        app.MapGet("/achievement-manager/user/{userId:guid}",
            async ([FromRoute] Guid userId,
                   [FromServices] ILogger<AchievementManagerEndpoint> log,
                   [FromServices] IAchievementManagerService achievementManagerService,
                   CancellationToken ct) =>
            {
                try
                {
                    var achievements = await achievementManagerService.GetUserAchievementsAsync(userId, ct);
                    return Results.Ok(achievements);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed to get achievements for user {UserId}", userId);
                    return Results.Problem("Failed to get achievements");
                }
            })
            .WithName("GetUserAchievements")
            .WithTags("Achievements")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapPost("/achievement-manager/track",
            async ([FromBody] TrackProgressRequest request,
                   [FromServices] ILogger<AchievementManagerEndpoint> log,
                   [FromServices] IAchievementManagerService achievementManagerService,
                   CancellationToken ct) =>
            {
                try
                {
                    await achievementManagerService.TrackProgressAsync(request, ct);
                    return Results.Ok(new { success = true });
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed to track progress for user {UserId}, feature {Feature}", request.UserId, request.Feature);
                    return Results.Problem("Failed to track progress");
                }
            })
            .WithName("TrackProgress")
            .WithTags("Achievements")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
