using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/media-accessor").WithTags("Media");

        group.MapGet("/speech/token", GetSpeechTokenAsync)
             .WithName("Accessor_GetSpeechToken");
        return app;
    }

    private static async Task<IResult> GetSpeechTokenAsync(
        [FromServices] ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        var logger = loggerFactory.CreateLogger("MediaEndpoints.SpeechToken");

        try
        {
            var region = "";
            var key = "";

            if (string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(key))
            {
                logger.LogWarning("Speech credentials missing (SPEECH_REGION / SPEECH_KEY)");
                return Results.Problem("Speech service not configured", statusCode: StatusCodes.Status500InternalServerError);
            }

            using var http = new HttpClient
            {
                BaseAddress = new Uri($"https://{region}.api.cognitive.microsoft.com/")
            };
            http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

            var resp = await http.PostAsync("sts/v1.0/issueToken", content: null, ct);
            if (!resp.IsSuccessStatusCode)
            {
                logger.LogError("Speech token request failed with {Status}", resp.StatusCode);
                return Results.Problem("Failed to acquire speech token", statusCode: (int)resp.StatusCode);
            }

            var token = await resp.Content.ReadAsStringAsync(ct);
            logger.LogInformation("Issued speech token for region {Region}, Token : {Token}", region, token);
            return Results.Ok(token);
        }
        catch (OperationCanceledException)
        {
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error getting speech token");
            return Results.Problem("Unexpected error retrieving speech token");
        }
    }
}
