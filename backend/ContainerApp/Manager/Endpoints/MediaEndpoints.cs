using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class MediaEndpoints
{
    private sealed class MediaEndpoint { }

    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/media-manager")
            .WithTags("Media")
            .RequireAuthorization();

        group.MapGet("/speech/token", GetSpeechTokenAsync)
             .WithName("GetSpeechToken");

        return app;
    }

    private static async Task<IResult> GetSpeechTokenAsync(
        [FromServices] Manager.Services.Clients.Accessor.IAccessorClient accessorClient,
        [FromServices] ILogger<MediaEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            var token = await accessorClient.GetSpeechTokenAsync(ct);
            if (string.IsNullOrWhiteSpace(token))
            {
                logger.LogWarning("Accessor returned empty speech token");
                return Results.Problem("Failed to retrieve speech token");
            }

            return Results.Ok(new { token });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invoking Accessor speech token endpoint");
            return Results.Problem("Failed to retrieve speech token");
        }
    }
}
