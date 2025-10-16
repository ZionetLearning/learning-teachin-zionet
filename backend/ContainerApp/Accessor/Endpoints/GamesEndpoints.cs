using Accessor.Services.Interfaces;
using Accessor.Models.Games;
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
        gamesGroup.MapPost("/generated-sentences", SaveGeneratedSentencesAsync);

        return app;
    }

    private static async Task<IResult> SubmitAttemptAsync(
        [FromBody] SubmitAttemptRequest request,
        [FromServices] IGameService service,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("SubmitAttemptAsync called. StudentId={StudentId}, GivenAnswer={GivenAnswer}", request.StudentId, string.Join(" ", request.GivenAnswer));

            var result = await service.SubmitAttemptAsync(request, ct);

            logger.LogInformation("SubmitAttemptAsync succeeded. StudentId={StudentId}, Status={Status}, AttemptNumber={AttemptNumber}", result.StudentId, result.Status, result.AttemptNumber);

            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
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
        [FromServices] IGameService service,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("GetAllHistoriesAsync called. Page={Page}, PageSize={PageSize}", page, pageSize);

            var result = await service.GetAllHistoriesAsync(page, pageSize, ct);

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
}