using Accessor.Models.WordCards;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class GameConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapWordCardsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/game-configuration-accessor").WithTags("Word Cards");

        group.MapGet("/{userId:guid}", GetWordCardsAsync);
        group.MapPost("/", CreateWordCardAsync);
        group.MapPatch("/learned", UpdateLearnedStatusAsync);

        return app;
    }

    private static async Task<IResult> GetWordCardsAsync(
        [FromRoute] Guid userId,
        [FromServices] IWordCardService wordCardservice,
        ILogger<IWordCardService> logger,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            logger.LogWarning("GetWordCardsAsync called with empty UserId");
            return Results.BadRequest("UserId cannot be empty.");
        }

        var scope = logger.BeginScope("GetWordCardsAsync. UserId={UserId}", userId);
        try
        {
            var result = await wordCardservice.GetWordCardsAsync(userId, ct);

            if (result == null || result.Count == 0)
            {
                logger.LogInformation("No word cards found for UserId={UserId}", userId);
                return Results.Ok(Array.Empty<WordCard>());
            }

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
        [FromBody] CreateWordCard createWordCardRequest,
        [FromServices] IWordCardService wordCardservice,
        ILogger<IWordCardService> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("CreateWordCardAsync");

        if (createWordCardRequest == null)
        {
            logger.LogWarning("CreateWordCardAsync called with null request");
            return Results.BadRequest("Request body cannot be null.");
        }

        try
        {
            logger.LogInformation("CreateWordCardAsync called. UserId={UserId}, Hebrew={Hebrew}, English={English}", createWordCardRequest.UserId, createWordCardRequest.Hebrew, createWordCardRequest.English);

            var result = await wordCardservice.CreateWordCardAsync(createWordCardRequest, ct);

            if (result == null)
            {
                logger.LogWarning("CreateWordCardAsync failed to create word card for UserId={UserId}", createWordCardRequest.UserId);
                return Results.Problem("Failed to create word card.");
            }

            logger.LogInformation("CreateWordCardAsync succeeded. CardId={CardId}", result.CardId);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in CreateWordCardAsync. UserId={UserId}", createWordCardRequest.UserId);
            return Results.Problem("Unexpected error occurred while creating word card.");
        }
    }

    private static async Task<IResult> UpdateLearnedStatusAsync(
        [FromBody] SetLearnedStatus request,
        [FromServices] IWordCardService wordCardservice,
        ILogger<IWordCardService> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("UpdateLearnedStatusAsync");

        if (request == null)
        {
            logger.LogWarning("UpdateLearnedStatusAsync called with null request");
            return Results.BadRequest("Request body cannot be null.");
        }

        try
        {
            logger.LogInformation("UpdateLearnedStatusAsync called. UserId={UserId}, CardId={CardId}, IsLearned={IsLearned}", request.UserId, request.CardId, request.IsLearned);

            var result = await wordCardservice.UpdateLearnedStatusAsync(request.UserId, request.CardId, request.IsLearned, ct);

            logger.LogInformation("UpdateLearnedStatusAsync succeeded. CardId={CardId}, IsLearned={IsLearned}", result.CardId, result.IsLearned);

            return Results.Ok(result);
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Card not found. CardId={CardId}", request.CardId);
            return Results.NotFound("Word card not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in UpdateLearnedStatusAsync. CardId={CardId}", request.CardId);
            return Results.Problem("Unexpected error occurred while updating learned status.");
        }
    }
}
