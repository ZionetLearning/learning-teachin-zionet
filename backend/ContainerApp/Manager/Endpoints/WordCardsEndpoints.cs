using System.Security.Claims;
using Manager.Constants;
using Manager.Mapping;
using Manager.Models.WordCards.Requests;
using Manager.Services.Clients.Accessor.Interfaces;
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

        wordCardsGroup.MapPatch("/learned", UpdateLearnedStatusAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        return app;
    }

    private static async Task<IResult> GetWordCardsAsync(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromServices] IWordCardsAccessorClient wordCardsAccessorClient,
        HttpContext http,
        ILogger<WordCardsEndpoint> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("GetWordCardsAsync. FromDate={FromDate}, ToDate={ToDate}", fromDate, toDate);
        try
        {
            var userIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);
            if (!Guid.TryParse(userIdRaw, out var userId))
            {
                logger.LogWarning("Invalid user ID in token");
                return Results.Unauthorized();
            }

            logger.LogInformation("Fetching word cards for UserId={UserId}, FromDate={FromDate}, ToDate={ToDate}",
                userId, fromDate, toDate);

            var accessorResponse = await wordCardsAccessorClient.GetWordCardsAsync(userId, fromDate, toDate, ct);
            var response = accessorResponse.ToFront();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching word cards");
            return Results.Problem("Failed to fetch word cards.");
        }
    }

    private static async Task<IResult> CreateWordCardAsync(
        [FromBody] CreateWordCardRequest request,
        [FromServices] IWordCardsAccessorClient wordCardsAccessorClient,
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

            var accessorRequest = request.ToAccessor(userId);
            var accessorResponse = await wordCardsAccessorClient.CreateWordCardAsync(accessorRequest, ct);
            var response = accessorResponse.ToFront();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating word card");
            return Results.Problem("Failed to create word card.");
        }
    }

    private static async Task<IResult> UpdateLearnedStatusAsync(
        [FromBody] UpdateLearnedStatusRequest request,
        [FromServices] IWordCardsAccessorClient wordCardsAccessorClient,
        HttpContext http,
        ILogger<WordCardsEndpoint> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("UpdateLearnedStatusAsync");
        try
        {
            var userIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);
            if (!Guid.TryParse(userIdRaw, out var userId))
            {
                logger.LogWarning("Invalid user ID in token");
                return Results.Unauthorized();
            }

            logger.LogInformation("Updating learned status. UserId={UserId}, CardId={CardId}, IsLearned={IsLearned}", userId, request.CardId, request.IsLearned);

            var accessorRequest = request.ToAccessor(userId);
            var accessorResponse = await wordCardsAccessorClient.UpdateLearnedStatusAsync(accessorRequest, ct);
            var response = accessorResponse.ToFront();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating learned status for CardId={CardId}", request.CardId);
            return Results.Problem("Failed to update learned status.");
        }
    }
}
