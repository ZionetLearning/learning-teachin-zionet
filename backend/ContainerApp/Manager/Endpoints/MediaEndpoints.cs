using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class MediaEndpoints
{
    private sealed class MediaEndpoint { }

    private const string AccessorAppId = "accessor"; // adjust if different
    private const string AccessorRoute = "media-accessor/speech/token";

    public static WebApplication MapMediaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/media-manager")
            .WithTags("Media")
            .RequireAuthorization();

        group.MapGet("/speech/token", GetSpeechTokenAsync)
             .WithName("GetSpeechToken");

        return app;
    }

    private static async Task<IResult> GetSpeechTokenAsync(
        [FromServices] DaprClient dapr,
        [FromServices] ILogger<MediaEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            // Invoke Accessor endpoint via Dapr service invocation
            var result = await dapr.InvokeMethodAsync<SpeechTokenResponse>(
                AccessorAppId,
                AccessorRoute,
                ct);

            if (result is null || string.IsNullOrWhiteSpace(result.token))
            {
                logger.LogWarning("Accessor returned empty speech token");
                return Results.Problem("Failed to retrieve speech token");
            }

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invoking Accessor speech token endpoint");
            return Results.Problem("Failed to retrieve speech token");
        }
    }

    private sealed record SpeechTokenResponse(string token, string region);
}
