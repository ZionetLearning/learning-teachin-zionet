using Dapr.Client;
using Manager.Constants;
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
        [FromServices] DaprClient dapr,
        [FromServices] ILogger<MediaEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            // Invoke Accessor endpoint via Dapr service invocation
            var token = await dapr.InvokeMethodAsync<string>(
                HttpMethod.Get,
                AppIds.Accessor,
                "media-accessor/speech/token",
                ct);

            if (token is null || string.IsNullOrWhiteSpace(token))
            {
                logger.LogWarning("Accessor returned empty speech token");
                return Results.Problem("Failed to retrieve speech token");
            }

            return Results.Ok(new { token = token });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invoking Accessor speech token endpoint");
            return Results.Problem("Failed to retrieve speech token");
        }
    }

    private sealed record SpeechTokenResponse(string token, string region);
}
