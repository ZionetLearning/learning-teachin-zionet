using Manager.Constants;
using Manager.Models.Achievements;
using Manager.Services;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class AchievementEndpoints
{
    private sealed class AchievementEndpoint { }

    public static IEndpointRouteBuilder MapAchievementEndpoints(this IEndpointRouteBuilder app)
    {
        var achievementsGroup = app.MapGroup("/achievements-manager").WithTags("Achievements");

        achievementsGroup.MapGet("/user/{userId:guid}",
            async ([FromRoute] Guid userId,
                   [FromServices] ILogger<AchievementEndpoint> log,
                   [FromServices] IAchievementService achievementService,
                   CancellationToken ct) =>
            {
                if (userId == Guid.Empty)
                {
                    log.LogWarning("Invalid userId provided");
                    return Results.BadRequest("Invalid userId");
                }

                try
                {
                    var achievements = await achievementService.GetUserAchievementsAsync(userId, ct);
                    return Results.Ok(achievements);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed to get achievements for user {UserId}", userId);
                    return Results.Problem("Failed to get achievements");
                }
            })
            .WithName("GetUserAchievements")
            .RequireAuthorization(PolicyNames.AdminOrStudent)
            .Produces<IReadOnlyList<AchievementDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

        achievementsGroup.MapPost("/track",
            async ([FromBody] TrackProgressRequest request,
                   [FromServices] ILogger<AchievementEndpoint> log,
                   [FromServices] IAchievementService achievementService,
                   CancellationToken ct) =>
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
                    var response = await achievementService.TrackProgressAsync(request, ct);
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed to track progress for user {UserId}, feature {Feature}", request.UserId, request.Feature);
                    return Results.Problem("Failed to track progress");
                }
            })
            .WithName("TrackProgress")
            .RequireAuthorization(PolicyNames.AdminOrStudent)
            .Produces<TrackProgressResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
