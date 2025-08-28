using Accessor.Services;
using Accessor.Models.RefreshSessions;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class RefreshSessionEndpoints
{
    public static void MapRefreshSessionEndpoints(this WebApplication app)
    {
        var refreshSessionGroup = app.MapGroup("/auth-accessor/refresh-sessions").WithTags("Auth");

        #region HTTP GET

        refreshSessionGroup.MapGet("/by-token-hash/{hash}", FindByRefreshHashAsync).WithName("FindRefreshSessionByHash");

        #endregion

        #region HTTP POST

        refreshSessionGroup.MapPost("", CreateSessionAsync).WithName("CreateRefreshSession");

        refreshSessionGroup.MapPost("/internal/cleanup", CleanupRefreshSessionsAsync)
            .WithName("CleanupRefreshSessions");

        #endregion

        #region HTTP PUT

        refreshSessionGroup.MapPut("/{sessionId:guid}/rotate", RotateSessionAsync).WithName("RotateRefreshSession");

        #endregion

        #region HTTP DELETE

        refreshSessionGroup.MapDelete("/{sessionId:guid}", DeleteSessionAsync).WithName("DeleteRefreshSession");

        #endregion
    }
    #region Handlers
    private static async Task<IResult> FindByRefreshHashAsync(
        [FromRoute] string hash,
        [FromServices] IRefreshSessionService refreshSessionService,
        [FromServices] ILogger<RefreshSessionService> logger,
        CancellationToken cancellationToken)
    {
        using (logger.BeginScope("Method: {Method}", nameof(FindByRefreshHashAsync)))
        {
            try
            {
                var session = await refreshSessionService.FindByRefreshHashAsync(hash, cancellationToken);
                if (session is null)
                {
                    logger.LogWarning("Refresh session not found for hash: {Hash}", hash);
                    return Results.NotFound();
                }

                return Results.Ok(session);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error finding refresh session by hash");
                return Results.Problem("Failed to find refresh session.");
            }
        }
    }
    private static async Task<IResult> CreateSessionAsync(
        [FromBody] RefreshSessionRequest request,
        [FromServices] IRefreshSessionService refreshSessionService,
        [FromServices] ILogger<RefreshSessionService> logger,
        CancellationToken cancellationToken)
    {
        using (logger.BeginScope("Method: {Method}", nameof(CreateSessionAsync)))
        {
            try
            {
                await refreshSessionService.CreateSessionAsync(request, cancellationToken);
                logger.LogInformation("Refresh session created for user {UserId}", request.UserId);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating refresh session");
                return Results.Problem("Failed to create refresh session.");
            }
        }
    }

    private static async Task<IResult> CleanupRefreshSessionsAsync(
    [FromServices] IRefreshSessionService refreshSessionService,
    [FromServices] ILogger<RefreshSessionService> logger,
    CancellationToken ct)
    {
        using (logger.BeginScope("Method: {Method}", nameof(CleanupRefreshSessionsAsync)))
        {
            try
            {
                // use a sensible default batch size; you can make it configurable later
                var deleted = await refreshSessionService.PurgeExpiredOrRevokedAsync(5000, ct);
                logger.LogInformation("Cleanup removed {Deleted} refresh sessions", deleted);
                return Results.Ok(new { deleted });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Cleanup failed");
                return Results.Problem("Cleanup failed.");
            }
        }
    }
    private static async Task<IResult> RotateSessionAsync(
        [FromRoute] Guid sessionId,
        [FromBody] RotateRefreshSessionRequest request,
        [FromServices] IRefreshSessionService refreshSessionService,
        [FromServices] ILogger<RefreshSessionService> logger,
        CancellationToken cancellationToken)
    {
        using (logger.BeginScope("Method: {Method}", nameof(RotateSessionAsync)))
        {
            try
            {
                await refreshSessionService.RotateSessionAsync(sessionId, request, cancellationToken);
                logger.LogInformation("Refresh session {SessionId} rotated", sessionId);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error rotating refresh session");
                return Results.Problem("Failed to rotate refresh session.");
            }
        }
    }

    private static async Task<IResult> DeleteSessionAsync(
        [FromRoute] Guid sessionId,
        [FromServices] IRefreshSessionService refreshSessionService,
        [FromServices] ILogger<RefreshSessionService> logger,
        CancellationToken cancellationToken)
    {
        using (logger.BeginScope("Method: {Method}", nameof(DeleteSessionAsync)))
        {
            try
            {
                await refreshSessionService.DeleteSessionAsync(sessionId, cancellationToken);
                logger.LogInformation("Refresh session {SessionId} deleted", sessionId);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting refresh session");
                return Results.Problem("Failed to delete refresh session.");
            }
        }
    }

    //private static async Task<IResult> DeleteAllUserSessionsAsync(
    //    [FromRoute] Guid userId,
    //    [FromServices] IRefreshSessionService refreshSessionService,
    //    [FromServices] ILogger<RefreshSessionService> logger,
    //    CancellationToken cancellationToken)
    //{
    //    using (logger.BeginScope("Method: {Method}", nameof(DeleteAllUserSessionsAsync)))
    //    {
    //        try
    //        {
    //            await refreshSessionService.DeleteAllUserSessionsAsync(userId, cancellationToken);
    //            logger.LogInformation("All refresh sessions deleted for user {UserId}", userId);
    //            return Results.Ok();
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.LogError(ex, "Error deleting all user refresh sessions");
    //            return Results.Problem("Failed to delete user sessions.");
    //        }
    //    }
    //}

    #endregion
}
