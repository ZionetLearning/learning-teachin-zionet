using System.Security.Claims;
using Manager.Constants;
using Manager.Models.WordCards;
using Manager.Services.Clients.Accessor;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class WordCardsEndpoints
{
    private sealed class WordCardsEndpoint { }

    public static IEndpointRouteBuilder MapWordCardsEndpoints(this IEndpointRouteBuilder app)
    {
        var wordCardsGroup = app.MapGroup("/wordcards-manager").WithTags("Word Cards");

        wordCardsGroup.MapGet("/", GetWordCardsAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        wordCardsGroup.MapPost("/", CreateWordCardAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        wordCardsGroup.MapPatch("/learned", MarkWordCardAsLearnedAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        return app;
    }

    private static async Task<IResult> GetWordCardsAsync(
        [FromServices] IAccessorClient accessorClient,
        HttpContext http,
        ILogger<WordCardsEndpoint> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("GetWordCardsAsync");
        try
        {
            var userIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);
            if (!Guid.TryParse(userIdRaw, out var userId))
            {
                logger.LogWarning("Invalid user ID in token");
                return Results.Unauthorized();
            }

            logger.LogInformation("Fetching word cards for UserId={UserId}", userId);

            var result = await accessorClient.GetWordCardsAsync(userId, ct);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching word cards");
            return Results.Problem("Failed to fetch word cards.");
        }
    }

    private static async Task<IResult> CreateWordCardAsync(
        [FromBody] CreateWordCardRequest request,
        [FromServices] IAccessorClient accessorClient,
        HttpContext http,
        ILogger<WordCardsEndpoint> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("CreateWordCardAsync");
        try
        {
            var userIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);
            if (!Guid.TryParse(userIdRaw, out var userId))
            {
                logger.LogWarning("Invalid user ID in token");
                return Results.Unauthorized();
            }

            logger.LogInformation("Creating word card for UserId={UserId}, Hebrew={Hebrew}, English={English}", userId, request.Hebrew, request.English);

            var result = await accessorClient.CreateWordCardAsync(userId, request, ct);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating word card");
            return Results.Problem("Failed to create word card.");
        }
    }

    private static async Task<IResult> MarkWordCardAsLearnedAsync(
        [FromBody] LearnedStatus request,
        [FromServices] IAccessorClient accessorClient,
        HttpContext http,
        ILogger<WordCardsEndpoint> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("MarkWordCardAsLearnedAsync");
        try
        {
            var userIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);
            if (!Guid.TryParse(userIdRaw, out var userId))
            {
                logger.LogWarning("Invalid user ID in token");
                return Results.Unauthorized();
            }

            logger.LogInformation("Marking word card as learned. UserId={UserId}, CardId={CardId}, IsLearned={IsLearned}", userId, request.CardId, request.IsLearned);

            var result = await accessorClient.UpdateLearnedStatusAsync(userId, request, ct);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating learned status for CardId={CardId}", request.CardId);
            return Results.Problem("Failed to update learned status.");
        }
    }
}
