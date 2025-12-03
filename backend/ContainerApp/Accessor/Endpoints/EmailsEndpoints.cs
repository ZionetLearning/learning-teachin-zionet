using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class EmailsEndpoints
{
    private sealed class EmailsEndpointsLoggerMarker { }

    public static IEndpointRouteBuilder MapEmailsEndpoints(this IEndpointRouteBuilder app)
    {
        var emailsGroup = app.MapGroup("/emails-accessor").WithTags("Emails");

        emailsGroup.MapGet("/recipients/{name}", GetRecipientEmailsByNameAsync)
            .WithName("GetRecipientEmailsByName");

        return app;
    }

    private static async Task<IResult> GetRecipientEmailsByNameAsync(
        [FromRoute] string name,
        [FromServices] IEmailService emailService,
        [FromServices] ILogger<EmailsEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("Method: {Method}, Name: {Name}", nameof(GetRecipientEmailsByNameAsync), name);

        if (string.IsNullOrWhiteSpace(name))
        {
            logger.LogWarning("Invalid name provided: {Name}", name);
            return Results.BadRequest("Name is required.");
        }

        try
        {
            var emails = await emailService.GetRecipientEmailsByNameAsync(name, ct);

            if (emails.Count == 0)
            {
                logger.LogInformation("No recipient emails found for name={Name}", name);
                return Results.NotFound(new { message = "No recipient emails found for the provided name." });
            }

            logger.LogInformation("Found {Count} recipient emails for name={Name}", emails.Count, name);
            return Results.Ok(emails);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve recipient emails for name={Name}", name);
            return Results.Problem("An error occurred while retrieving recipient emails.");
        }
    }
}

