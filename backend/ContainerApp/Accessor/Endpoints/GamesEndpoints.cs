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
        gamesGroup.MapPost("/generated-sentence", SaveGeneratedSentenceAsync);

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
        [FromServices] IGameService service,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("GetHistoryAsync called. StudentId={StudentId}, Summary={Summary}, Page={Page}, PageSize={PageSize}", studentId, summary, page, pageSize
            );

            var result = await service.GetHistoryAsync(studentId, summary, page, pageSize, ct);

            logger.LogInformation("GetHistoryAsync returned {Records} records (page). TotalCount={TotalCount}", result.Items.Count(), result.TotalCount);

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

    private static async Task<IResult> SaveGeneratedSentenceAsync(
        [FromBody] GeneratedSentenceDto dto,
        [FromServices] IGameService gameService,
        ILogger<IGameService> logger,
        CancellationToken ct)
    {
        try
        {
            await gameService.SaveGeneratedSentenceAsync(dto, ct);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving generated sentence for StudentId={StudentId}", dto.StudentId);
            return Results.Problem("Failed to save generated sentence.");
        }
    }
}