using Microsoft.AspNetCore.Mvc;
using Manager.Mapping;
using Manager.Services.Clients.Accessor.Interfaces;

namespace Manager.Endpoints;

public static class MediaEndpoints
{
    private sealed class MediaEndpoint { }

    public static void MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/media-manager")
            .WithTags("Media")
            .RequireAuthorization();

        group.MapGet("/speech/token", GetSpeechTokenAsync)
             .WithName("GetSpeechToken");
    }

    private static async Task<IResult> GetSpeechTokenAsync(
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<MediaEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            var accessorResponse = await accessorClient.GetSpeechTokenAsync(ct);
            var response = accessorResponse.ToFront();

            if (!string.IsNullOrWhiteSpace(response.Token))
            {
                return Results.Ok(response);
            }

            logger.LogError("Accessor returned empty speech token");
            return Results.Problem("Failed to retrieve speech token");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invoking Accessor speech token endpoint");
            return Results.Problem("Failed to retrieve speech token");
        }
    }
}
