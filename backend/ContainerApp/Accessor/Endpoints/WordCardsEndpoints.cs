using Accessor.Models.WordCards;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class WordCardsEndpoints
{
    public static IEndpointRouteBuilder MapWordCardsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/wordcards-accessor").WithTags("Word Cards");

        group.MapGet("/{userId:guid}", GetWordCardsAsync);
        group.MapPost("/", CreateWordCardAsync);
        group.MapPatch("/{cardId:guid}/learned", UpdateLearnedStatusAsync);

        return app;
    }

    private static async Task<IResult> GetWordCardsAsync(
        [FromRoute] Guid userId,
        [FromServices] IWordCardService service,
        ILogger<IWordCardService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("GetWordCardsAsync called. UserId={UserId}", userId);

            var result = await service.GetWordCardsAsync(userId, ct);

            logger.LogInformation("GetWordCardsAsync returned {Count} cards for UserId={UserId}", result.Count, userId);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in GetWordCardsAsync. UserId={UserId}", userId);
            return Results.Problem("Unexpected error occurred while fetching word cards.");
        }
    }

    private static async Task<IResult> CreateWordCardAsync(
        [FromBody] CreateWordCardInternalRequest request,
        [FromServices] IWordCardService service,
        ILogger<IWordCardService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("CreateWordCardAsync called. UserId={UserId}, Hebrew={Hebrew}, English={English}", request.UserId, request.Hebrew, request.English);

            var result = await service.CreateWordCardAsync(request, ct);

            logger.LogInformation("CreateWordCardAsync succeeded. CardId={CardId}", result.CardId);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in CreateWordCardAsync. UserId={UserId}", request.UserId);
            return Results.Problem("Unexpected error occurred while creating word card.");
        }
    }

    private static async Task<IResult> UpdateLearnedStatusAsync(
        [FromRoute] Guid cardId,
        [FromQuery] Guid userId,
        [FromBody] SetLearnedStatusRequest request,
        [FromServices] IWordCardService service,
        ILogger<IWordCardService> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("UpdateLearnedStatusAsync called. UserId={UserId}, CardId={CardId}, IsLearned={IsLearned}", userId, cardId, request.IsLearned);

            var result = await service.UpdateLearnedStatusAsync(userId, cardId, request.IsLearned, ct);

            logger.LogInformation("UpdateLearnedStatusAsync succeeded. CardId={CardId}, IsLearned={IsLearned}", result.CardId, result.IsLearned);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in UpdateLearnedStatusAsync. CardId={CardId}", cardId);
            return Results.Problem("Unexpected error occurred while updating learned status.");
        }
    }
}
