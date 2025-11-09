using Accessor.Models.Games;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class GamesEndpoints
{
    public static IEndpointRouteBuilder MapGamesEndpoints(this IEndpointRouteBuilder app)
    {
        var gamesGroup = app.MapGroup("/games-accessor").WithTags("Games");

        gamesGroup.MapPost("/attempt", SubmitAttemptAsync);
        gamesGroup.MapGet("/history/{studentId:guid}", GetHistoryAsync);
        gamesGroup.MapGet("/mistakes/{studentId:guid}", GetMistakesAsync);
        gamesGroup.MapGet("/all-history", GetAllHistoriesAsync);
        gamesGroup.MapGet("/attempt/{userId:guid}/{attemptId:guid}", GetAttemptDetailsAsync);
        gamesGroup.MapGet("/attempt/last/{userId:guid}/{gameType}", GetLastAttemptAsync);
        gamesGroup.MapPost("/generated-sentences", SaveGeneratedSentencesAsync);
        gamesGroup.MapDelete("/all-history", DeleteAllGamesHistoryAsync);

        return app;
    }

    private static async Task<IResult> SubmitAttemptAsync(
        [FromBody] SubmitAttemptRequest request,
        [FromServices] IGameService service,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        if (request is null)
        {
            logger.LogWarning("SubmitAttemptAsync rejected. Request body is null.");
            return Results.BadRequest(new { message = "Request body must not be null." });
        }

        if (request.AttemptId == Guid.Empty)
        {
            logger.LogWarning("SubmitAttemptAsync rejected. Invalid AttemptId provided.");
            return Results.BadRequest(new { message = "AttemptId must be a non-empty GUID." });
        }

        using var scope = logger.BeginScope("Method: {Method}, AttemptId: {AttemptId}", nameof(SubmitAttemptAsync), request.AttemptId);

        try
        {
            var result = await service.SubmitAttemptAsync(request, ct);

            logger.LogInformation("SubmitAttemptAsync succeeded. StudentId={StudentId}, Status={Status}, AttemptNumber={AttemptNumber}", result.StudentId, result.Status, result.AttemptNumber);

            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Validation failed. AttemptId={AttemptId}", request.AttemptId);
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "SubmitAttemptAsync failed. StudentId={StudentId} - Game not found", request.StudentId);
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in SubmitAttemptAsync. StudentId={StudentId}", request.StudentId);
            return Results.Problem("Unexpected error occurred while submitting attempt.");
        }
    }

    private static async Task<IResult> GetHistoryAsync(
        [FromRoute] Guid studentId,
        [FromQuery] bool summary,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] bool getPending,
        [FromServices] IGameService service,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("GetHistoryAsync called. StudentId={StudentId}, Summary={Summary}, GetPending={GetPending}, Page={Page}, PageSize={PageSize}", studentId, summary, getPending, page, pageSize
            );

            var result = await service.GetHistoryAsync(studentId, summary, page, pageSize, getPending, ct);

            logger.LogInformation("GetHistoryAsync returned {Records} records (page). TotalCount={TotalCount}", result.Items.Count(), result.TotalCount);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetHistoryAsync. StudentId={StudentId}, Summary={Summary}, GetPending={GetPending}", studentId, summary, getPending);
            return Results.Problem("Unexpected error occurred while fetching history.");
        }
    }

    private static async Task<IResult> GetMistakesAsync(
        [FromRoute] Guid studentId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] IGameService service,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("GetMistakesAsync called. StudentId={StudentId}, Page={Page}, PageSize={PageSize}", studentId, page, pageSize);

            var result = await service.GetMistakesAsync(studentId, page, pageSize, ct);

            logger.LogInformation("GetMistakesAsync returned {Records} mistakes (page). TotalCount={TotalCount}", result.Items.Count(), result.TotalCount);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error in GetMistakesAsync. StudentId={StudentId}",
                studentId
            );
            return Results.Problem("Unexpected error occurred while fetching mistakes.");
        }
    }

    private static async Task<IResult> GetAllHistoriesAsync(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] IStudentPracticeHistoryService service,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("GetAllHistoriesAsync called. Page={Page}, PageSize={PageSize}", page, pageSize);

            var result = await service.GetHistoryAsync(page, pageSize, ct);

            logger.LogInformation("GetAllHistoriesAsync returned {Records} records (page). TotalCount={TotalCount}", result.Items.Count(), result.TotalCount);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetAllHistoriesAsync");
            return Results.Problem("Unexpected error occurred while fetching all histories.");
        }
    }

    private static async Task<IResult> GetAttemptDetailsAsync(
        [FromRoute] Guid userId,
        [FromRoute] Guid attemptId,
        [FromServices] IGameService service,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("GetAttemptDetailsAsync called. UserId={UserId}, AttemptId={AttemptId}", userId, attemptId);

            var result = await service.GetAttemptDetailsAsync(userId, attemptId, ct);

            logger.LogInformation("GetAttemptDetailsAsync succeeded. UserId={UserId}, AttemptId={AttemptId}, GameType={GameType}", userId, attemptId, result.GameType);

            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "GetAttemptDetailsAsync failed. UserId={UserId}, AttemptId={AttemptId} - Attempt not found", userId, attemptId);
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in GetAttemptDetailsAsync. UserId={UserId}, AttemptId={AttemptId}", userId, attemptId);
            return Results.Problem("Unexpected error occurred while fetching attempt details.");
        }
    }

    private static async Task<IResult> SaveGeneratedSentencesAsync(
        [FromBody] GeneratedSentenceDto dto,
        [FromServices] IGameService gameService,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        try
        {
            var result = await gameService.SaveGeneratedSentencesAsync(dto, ct);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving generated sentence for StudentId={StudentId}", dto.StudentId);
            return Results.Problem("Failed to save generated sentence.");
        }
    }

    private static async Task<IResult> DeleteAllGamesHistoryAsync(
       [FromServices] IGameService gameService,
       ILogger<IGameService> logger,
       CancellationToken ct)
    {
        try
        {
            await gameService.DeleteAllGamesHistoryAsync(ct);
            logger.LogInformation("All games history deleted successfully.");
            return Results.Ok(new { message = "All games history deleted." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting games history.");
            return Results.Problem("Failed to delete all games history.");
        }
    }

    private static async Task<IResult> GetLastAttemptAsync(
        [FromRoute] Guid userId,
        [FromRoute] string gameType,
        [FromServices] IGameService service,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("GetLastAttemptAsync called. UserId={UserId}", userId);

            var result = await service.GetLastAttemptAsync(userId, gameType, ct);

            logger.LogInformation(
                "GetLastAttemptAsync succeeded. UserId={UserId}, AttemptId={AttemptId}, GameType={GameType}",
                userId, result.AttemptId, result.GameType
            );

            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(
                ex,
                "GetLastAttemptAsync failed. UserId={UserId} - No attempts found",
                userId
            );
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error in GetLastAttemptAsync. UserId={UserId}",
                userId
            );
            return Results.Problem("Unexpected error occurred while fetching last attempt.");
        }
    }
}