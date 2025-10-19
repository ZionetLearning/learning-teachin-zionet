using System.Security.Claims;
using Manager.Constants;
using Manager.Models.Users;
using Manager.Models.Games;
using Manager.Services.Clients.Accessor;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class GamesEndpoints
{
    private sealed class GameEndpoint { }

    public static IEndpointRouteBuilder MapGamesEndpoints(this IEndpointRouteBuilder app)
    {
        var gamesGroup = app.MapGroup("/games-manager").WithTags("Games");

        gamesGroup.MapPost("/attempt", SubmitAttemptAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        gamesGroup.MapGet("/history/{studentId:guid}", GetHistoryAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        gamesGroup.MapGet("/mistakes/{studentId:guid}", GetMistakesAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        gamesGroup.MapGet("/all-history", GetAllHistoriesAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacher);

        gamesGroup.MapDelete("/all-history", DeleteAllGamesHistoryAsync)
            .RequireAuthorization(PolicyNames.AdminOnly);

        return app;
    }

    private static async Task<IResult> SubmitAttemptAsync(
        [FromBody] SubmitAttemptRequest request,
        [FromServices] IAccessorClient accessorClient,
        HttpContext http,
        ILogger<GameEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            var callerRole = http.User.FindFirstValue(AuthSettings.RoleClaimType);
            var callerIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

            logger.LogInformation("SubmitAttempt called by role={Role}, callerId={CallerId}, studentId={StudentId}", callerRole, callerIdRaw, request.StudentId);

            if (callerRole?.Equals(Role.Student.ToString(), StringComparison.OrdinalIgnoreCase) == true &&
                Guid.TryParse(callerIdRaw, out var callerId) && callerId != request.StudentId)
            {
                logger.LogWarning("Forbidden attempt: Student {CallerId} tried to submit attempt for Student {StudentId}", callerId, request.StudentId);
                return Results.Forbid();
            }

            var result = await accessorClient.SubmitAttemptAsync(request, ct);

            logger.LogInformation("Attempt submitted successfully for StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}, Status={Status}, AttemptNumber={AttemptNumber}", result.StudentId, result.GameType, result.Difficulty, result.Status, result.AttemptNumber);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while submitting attempt for StudentId={StudentId}", request.StudentId);
            return Results.Problem("Failed to submit attempt. Please try again later.");
        }
    }

    private static async Task<IResult> GetHistoryAsync(
        [FromRoute] Guid studentId,
        [FromQuery] bool summary,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] bool getPending,
        [FromServices] IAccessorClient accessorClient,
        HttpContext http,
        ILogger<GameEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            var callerRole = http.User.FindFirstValue(AuthSettings.RoleClaimType);
            var callerIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

            if (callerRole?.Equals(Role.Student.ToString(), StringComparison.OrdinalIgnoreCase) == true &&
                Guid.TryParse(callerIdRaw, out var callerId) && callerId != studentId)
            {
                logger.LogWarning("Forbidden history access: Student {CallerId} tried to view Student {StudentId}", callerId, studentId);
                return Results.Forbid();
            }

            logger.LogInformation("Fetching history for StudentId={StudentId}, Summary={Summary}, GetPending={GetPending}, Page={Page}, PageSize={PageSize}", studentId, summary, getPending, page, pageSize);

            var result = await accessorClient.GetHistoryAsync(studentId, summary, page, pageSize, getPending, ct);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching history for StudentId={StudentId}", studentId);
            return Results.Problem("Failed to fetch history. Please try again later.");
        }
    }

    private static async Task<IResult> GetMistakesAsync(
        [FromRoute] Guid studentId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] IAccessorClient accessorClient,
        HttpContext http,
        ILogger<GameEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            var callerRole = http.User.FindFirstValue(AuthSettings.RoleClaimType);
            var callerIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

            if (callerRole?.Equals(Role.Student.ToString(), StringComparison.OrdinalIgnoreCase) == true &&
                Guid.TryParse(callerIdRaw, out var callerId) && callerId != studentId)
            {
                logger.LogWarning("Forbidden mistakes access: Student {CallerId} tried to view Student {StudentId}", callerId, studentId);
                return Results.Forbid();
            }

            logger.LogInformation("Fetching mistakes for StudentId={StudentId}, Page={Page}, PageSize={PageSize}", studentId, page, pageSize);

            var result = await accessorClient.GetMistakesAsync(studentId, page, pageSize, ct);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching mistakes for StudentId={StudentId}", studentId);
            return Results.Problem("Failed to fetch mistakes. Please try again later.");
        }
    }

    private static async Task<IResult> GetAllHistoriesAsync(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] IAccessorClient accessorClient,
        ILogger<GameEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Fetching all histories Page={Page}, PageSize={PageSize}", page, pageSize);

            var result = await accessorClient.GetAllHistoriesAsync(page, pageSize, ct);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all histories");
            return Results.Problem("Failed to fetch all histories. Please try again later.");
        }
    }

    private static async Task<IResult> DeleteAllGamesHistoryAsync(
        [FromServices] IAccessorClient accessorClient,
        ILogger<GameEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Request received to delete all games history.");
            var success = await accessorClient.DeleteAllGamesHistoryAsync(ct);

            if (success)
            {
                logger.LogInformation("All games history deleted successfully.");
                return Results.Ok(new { message = "All games history deleted." });
            }

            return Results.Problem("Failed to delete games history.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting all games history");
            return Results.Problem("Failed to delete games history.");
        }
    }
}