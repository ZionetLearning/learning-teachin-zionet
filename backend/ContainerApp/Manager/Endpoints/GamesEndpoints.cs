using System.Security.Claims;
using Manager.Constants;
using Manager.Mapping;
using Manager.Models.Games;
using Manager.Models.ModelValidation;
using Manager.Models.Users;
using Manager.Services.Clients.Accessor.Interfaces;
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
        [FromServices] IGameAccessorClient gameAccessorClient,
        HttpContext http,
        ILogger<GameEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            var callerRole = http.User.FindFirstValue(AuthSettings.RoleClaimType);
            var callerIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);
            if (!ValidationExtensions.TryValidate(request, out var validationErrors))
            {
                logger.LogWarning("Validation failed for {Model}: {Errors}", nameof(SubmitAttemptRequest), validationErrors);
                return Results.BadRequest(new { errors = validationErrors });
            }

            if (!Guid.TryParse(callerIdRaw, out var studentId))
            {
                logger.LogWarning("Invalid or missing UserId in token: {CallerIdRaw}", callerIdRaw);
                return Results.Unauthorized();
            }

            logger.LogInformation("SubmitAttempts called by role={Role}, studentId={StudentId}", callerRole, studentId);

            var accessorResult = await gameAccessorClient.SubmitAttemptAsync(studentId, request, ct);
            var response = accessorResult.ToApiModel();

            logger.LogInformation(
                "Attempt submitted successfully for StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}, Status={Status}, AttemptNumber={AttemptNumber}",
                response.StudentId, response.GameType, response.Difficulty, response.Status, response.AttemptNumber);

            return Results.Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation("Exercise not found. ExerciseId={ExerciseId}", request.ExerciseId);
            return Results.NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid attempt submission. ExerciseId={ExerciseId}", request.ExerciseId);
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while submitting attempt. ExerciseId={ExerciseId}", request.ExerciseId);
            return Results.Problem("Failed to submit attempt. Please try again later.");
        }
    }

    private static async Task<IResult> GetHistoryAsync(
        [FromRoute] Guid studentId,
        [FromQuery] bool summary,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] bool getPending,
        [FromServices] IGameAccessorClient gameAccessorClient,
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

            var accessorResult = await gameAccessorClient.GetHistoryAsync(studentId, summary, page, pageSize, getPending, ct);
            var response = accessorResult.ToApiModel();

            if (response.IsSummary)
            {
                logger.LogInformation("Returned {Records} summary records", response.Summary?.Items.Count ?? 0);
                return Results.Ok(response.Summary);
            }
            else
            {
                logger.LogInformation("Returned {Records} detailed records", response.Detailed?.Items.Count ?? 0);
                return Results.Ok(response.Detailed);
            }
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
        [FromServices] IGameAccessorClient gameAccessorClient,
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

            var accessorResult = await gameAccessorClient.GetMistakesAsync(studentId, page, pageSize, ct);
            var response = accessorResult.ToApiModel();

            return Results.Ok(response);
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
        [FromServices] IGameAccessorClient gameAccessorClient,
        ILogger<GameEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Fetching all histories Page={Page}, PageSize={PageSize}", page, pageSize);

            var accessorResult = await gameAccessorClient.GetAllHistoriesAsync(page, pageSize, ct);
            var response = accessorResult.ToApiModel();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all histories");
            return Results.Problem("Failed to fetch all histories. Please try again later.");
        }
    }

    private static async Task<IResult> DeleteAllGamesHistoryAsync(
        [FromServices] IGameAccessorClient gameAccessorClient,
        ILogger<GameEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Request received to delete all games history.");
            var success = await gameAccessorClient.DeleteAllGamesHistoryAsync(ct);

            if (success)
            {
                logger.LogInformation("All games history deleted successfully.");
                var response = new DeleteAllGamesHistoryResponse { Message = "All games history deleted." };
                return Results.Ok(response);
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