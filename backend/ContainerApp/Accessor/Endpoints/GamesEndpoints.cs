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
            logger.LogInformation("SubmitAttemptAsync called. StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}, GivenAnswer={GivenAnswer}", request.StudentId, request.GameType, request.Difficulty, string.Join(" ", request.GivenAnswer));

            var result = await service.SubmitAttemptAsync(request, ct);

            logger.LogInformation("SubmitAttemptAsync succeeded. StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}, Success={IsSuccess}, AttemptNumber={AttemptNumber}", result.StudentId, result.GameType, result.Difficulty, result.IsSuccess, result.AttemptNumber);

            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "SubmitAttemptAsync failed. StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty} - Game not found", request.StudentId, request.GameType, request.Difficulty);
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in SubmitAttemptAsync. StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}", request.StudentId, request.GameType, request.Difficulty);
            return Results.Problem("Unexpected error occurred while submitting attempt.");
        }
    }

    private static async Task<IResult> GetHistoryAsync(
        [FromRoute] Guid studentId,
        [FromQuery] bool summary,
        [FromServices] IGameService service,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("GetHistoryAsync called. StudentId={StudentId}, Summary={Summary}", studentId, summary);

            var result = await service.GetHistoryAsync(studentId, summary, ct);

            logger.LogInformation("GetHistoryAsync returned {Count} records. StudentId={StudentId}, Summary={Summary}", result.Count(), studentId, summary);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetHistoryAsync. StudentId={StudentId}, Summary={Summary}", studentId, summary);
            return Results.Problem("Unexpected error occurred while fetching history.");
        }
    }

    private static async Task<IResult> GetMistakesAsync(
        [FromRoute] Guid studentId,
        [FromServices] IGameService service,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("GetMistakesAsync called. StudentId={StudentId}", studentId);

            var result = await service.GetMistakesAsync(studentId, ct);

            logger.LogInformation("GetMistakesAsync returned {Count} mistakes. StudentId={StudentId}", result.Count(), studentId);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetMistakesAsync. StudentId={StudentId}", studentId);
            return Results.Problem("Unexpected error occurred while fetching mistakes.");
        }
    }

    private static async Task<IResult> GetAllHistoriesAsync(
        [FromServices] IGameService service,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("GetAllHistoriesAsync called");

            var result = await service.GetAllHistoriesAsync(ct);

            logger.LogInformation("GetAllHistoriesAsync returned {Count} records", result.Count());

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetAllHistoriesAsync");
            return Results.Problem("Unexpected error occurred while fetching all histories.");
        }
    }
}