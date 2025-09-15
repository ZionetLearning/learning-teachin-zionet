using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class MediaEndpoints
{
    public static void MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/media-accessor").WithTags("Media");

        group.MapGet("/speech/token", GetSpeechTokenAsync)
             .WithName("Accessor_GetSpeechToken");
    }

    public static async Task<IResult> GetSpeechTokenAsync(
        [FromServices] ISpeechService speechService,
        CancellationToken ct)
    {
        try
        {
            var speechTokenData = await speechService.GetSpeechTokenAsync(ct);
            return Results.Ok(speechTokenData);
        }
        catch (OperationCanceledException)
        {
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem($"Failed to acquire speech token: {ex.Message}", statusCode: StatusCodes.Status502BadGateway);
        }
        catch (Exception)
        {
            return Results.Problem("Unexpected error retrieving speech token");
        }
    }
}
