using Accessor.Models.GameConfiguration;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class UserGameConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapWordCardsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/game-config-accessor").WithTags("Game Config Accessor");

        group.MapGet("/", GetUserGameConfigAsync);
        group.MapPut("/", SaveUserGameConfigAsync);
        group.MapDelete("/", DeleteUserGameConfigAsync);

        return app;
    }

    private static async Task<IResult> GetUserGameConfigAsync(
       [FromQuery] Guid userId,
       [FromQuery] GameName gameName,
       [FromServices] IUserGameConfigurationService service,
       ILogger<IUserGameConfigurationService> logger,
       CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            logger.LogWarning("GetUserGameConfigAsync called with empty UserId");
            return Results.BadRequest("UserId cannot be empty.");
        }

        using var scope = logger.BeginScope("GetUserGameConfigAsync: UserId={UserId}, GameName={GameName}", userId, gameName);

        try
        {
            var config = await service.GetGameConfigAsync(userId, gameName, ct);

            if (config is null)
            {
                logger.LogInformation("No config found for UserId={UserId}, GameName={GameName}", userId, gameName);
                return Results.NotFound(new { error = "Game configuration not found." });
            }

            return Results.Ok(config);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in GetUserGameConfigAsync. UserId={UserId}", userId);
            return Results.Problem("Unexpected error occurred while retrieving game configuration.");
        }
    }

    private static async Task<IResult> SaveUserGameConfigAsync(
        [FromBody] UserGameConfig userGameConfig,
        [FromServices] IUserGameConfigurationService service,
        ILogger<IUserGameConfigurationService> logger,
        CancellationToken ct)
    {
        if (userGameConfig == null)
        {
            logger.LogWarning("SaveUserGameConfigAsync called with null request");
            return Results.BadRequest("Request body cannot be null.");
        }

        using var scope = logger.BeginScope("SaveUserGameConfigAsync: UserId={UserId}, GameName={GameName}", userGameConfig.UserId, userGameConfig.GameName);

        try
        {
            await service.SaveConfigAsync(userGameConfig, ct);
            logger.LogInformation("Game config saved successfully for UserId={UserId}, GameName={GameName}", userGameConfig.UserId, userGameConfig.GameName);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in SaveUserGameConfigAsync. UserId={UserId}", userGameConfig.UserId);
            return Results.Problem("Unexpected error occurred while saving game configuration.");
        }
    }

    private static async Task<IResult> DeleteUserGameConfigAsync(
        [FromBody] UserGameConfigKey request,
        [FromServices] IUserGameConfigurationService service,
        ILogger<IUserGameConfigurationService> logger,
        CancellationToken ct)
    {
        if (request == null || request.UserId == Guid.Empty)
        {
            logger.LogWarning("DeleteUserGameConfigAsync called with invalid request");
            return Results.BadRequest("Invalid request body.");
        }

        using var scope = logger.BeginScope("DeleteUserGameConfigAsync: UserId={UserId}, GameName={GameName}", request.UserId, request.GameName);

        try
        {
            await service.DeleteConfigAsync(request.UserId, request.GameName, ct);
            logger.LogInformation("Game config deleted for UserId={UserId}, GameName={GameName}", request.UserId, request.GameName);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in DeleteUserGameConfigAsync. UserId={UserId}", request.UserId);
            return Results.Problem("Unexpected error occurred while deleting game configuration.");
        }
    }
}
