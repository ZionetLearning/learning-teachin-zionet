using System.Security.Claims;
using Manager.Constants;
using Microsoft.AspNetCore.Mvc;
using Manager.Models.GameConfiguration;
using Manager.Services.Clients.Accessor;

namespace Manager.Endpoints;

public static class GameConfigurationEndpoints
{
    private sealed class GameConfigurationEndpoint();
    public static IEndpointRouteBuilder MapGameConfigEndpoints(this IEndpointRouteBuilder app)
    {
        var configGroup = app.MapGroup("/game-config-manager").WithTags("Game Configuration");

        configGroup.MapGet("/{gameName}", GetConfigAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        configGroup.MapPut("/{gameName}", SaveConfigAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        configGroup.MapDelete("/{gameName}", DeleteConfigAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        return app;
    }

    private static async Task<IResult> GetConfigAsync(
        [FromRoute] GameName gameName,
        [FromServices] IAccessorClient accessorClient,
        HttpContext http,
        [FromServices] ILogger<GameConfigurationEndpoint> logger,
        CancellationToken ct)
    {
        var scope = logger.BeginScope("GetConfigAsync");

        try
        {
            var userIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

            if (!Guid.TryParse(userIdRaw, out var userId))
            {
                logger.LogWarning("Invalid or missing User ID in claims: {RawUserId}", userIdRaw);
                return Results.Unauthorized();
            }

            var config = await accessorClient.GetUserGameConfigAsync(userId, gameName, ct);
            if (config == null)
            {
                logger.LogInformation("No configuration found for user {UserId} and game {GameName}", userId, gameName);
                return Results.NotFound(new { error = "Game configuration not found" });
            }

            return Results.Ok(config);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving game configuration for game {GameName}", gameName);
            return Results.Problem("Failed to retrieve game configuration. Please try again later.");
        }
    }

    private static async Task<IResult> SaveConfigAsync(
        [FromBody] UserNewGameConfig userNewGameConfig,
        [FromServices] IAccessorClient accessorClient,
        HttpContext http,
        [FromServices] ILogger<GameConfigurationEndpoint> logger,
        CancellationToken ct)
    {
        var scope = logger.BeginScope("SaveConfigAsync");
        try
        {
            var userIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

            if (!Guid.TryParse(userIdRaw, out var userId))
            {
                logger.LogWarning("Invalid or missing User ID in claims: {RawUserId}", userIdRaw);
                return Results.Unauthorized();
            }

            await accessorClient.SaveUserGameConfigAsync(userId, userNewGameConfig, ct);
            return Results.Ok(new { message = "Configuration saved." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving game configuration for game {GameName}", userNewGameConfig.GameName);
            return Results.Problem("Failed to save game configuration. Please try again later.");
        }
    }

    private static async Task<IResult> DeleteConfigAsync(
        [FromRoute] GameName gameName,
        [FromServices] IAccessorClient accessorClient,
        HttpContext http,
        [FromServices] ILogger<GameConfigurationEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            var userIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

            if (!Guid.TryParse(userIdRaw, out var userId))
            {
                logger.LogWarning("Invalid or missing User ID in claims: {RawUserId}", userIdRaw);
                return Results.Unauthorized();
            }

            await accessorClient.DeleteUserGameConfigAsync(userId, gameName, ct);
            return Results.Ok(new { message = "Configuration saved." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving game configuration for game {GameName}", gameName);
            return Results.Problem("Failed to save game configuration. Please try again later.");
        }
    }
}
