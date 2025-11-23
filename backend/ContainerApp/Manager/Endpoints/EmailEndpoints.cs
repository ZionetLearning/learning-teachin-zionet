// using System.Security.Claims;
using Manager.Constants;
// using Manager.Models.Emails;
using Manager.Services.Clients.Accessor.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class EmailEndpoints
{
    public sealed class EmailsEndpoint;

    public static IEndpointRouteBuilder MapEmailEndpoints(this IEndpointRouteBuilder app)
    {
        var emailsGroup = app.MapGroup("/emails-manager").WithTags("Emails");

        emailsGroup.MapGet("/recipients/{name}", GetRecipientEmailsByName)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        // emailsGroup.MapPost("/draft", CreateEmailDraftAsync)
        //     .RequireAuthorization(PolicyNames.AdminOrTeacher);

        // emailsGroup.MapPost("/send", SendEmailAsync)
        //     .RequireAuthorization(PolicyNames.AdminOrTeacher);

        return app;
    }

    private static async Task<IResult> GetRecipientEmailsByName(
        [FromServices] IEmailAccessorClient emailAccessorClient,
        [FromRoute] string name,
        ILogger<EmailsEndpoint> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("GetRecipientEmailsByName");
        try
        {
            logger.LogInformation("Looking up recipient emails for name={Name}", name);

            var emails = await emailAccessorClient.GetRecipientEmailsByNameAsync(name, ct);

            if (emails.Count == 0)
            {
                logger.LogWarning("No emails found for name={Name}", name);
                return Results.NotFound();
            }

            return Results.Ok(new { emails });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving recipient email for name={Name}", name);
            return Results.Problem("Failed to retrieve recipient email.");
        }
    }

    // private static async Task<IResult> CreateEmailDraftAsync(
    //     [FromBody] AiGeneratedEmailRequest request,
    //     [FromServices] IAccessorClient accessorClient,
    //     HttpContext http,
    //     ILogger<EmailsEndpoint> logger,
    //     CancellationToken ct)
    // {
    //     using var scope = logger.BeginScope("CreateEmailDraftAsync");

    //     try
    //     {
    //         var userIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);
    //         if (!Guid.TryParse(userIdRaw, out var userId))
    //         {
    //             logger.LogWarning("Invalid user ID in token");
    //             return Results.Unauthorized();
    //         }

    //         logger.LogInformation("Generating AI email draft. UserId={UserId}, Purpose={Purpose}", userId, request.Purpose);

    //         var draft = await accessorClient.GenerateEmailDraftAsync(userId, request, ct);

    //         return Results.Ok(draft);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Failed to generate AI email draft");
    //         return Results.Problem("Email draft generation failed.");
    //     }
    // }

    // private static async Task<IResult> SendEmailAsync(
    //     [FromBody] SendEmailRequest request,
    //     [FromServices] IAccessorClient accessorClient,
    //     HttpContext http,
    //     ILogger<EmailsEndpoint> logger,
    //     CancellationToken ct)
    // {
    //     using var scope = logger.BeginScope("SendEmailAsync");

    //     try
    //     {
    //         var userIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);
    //         if (!Guid.TryParse(userIdRaw, out var userId))
    //         {
    //             logger.LogWarning("Invalid user ID in token");
    //             return Results.Unauthorized();
    //         }

    //         logger.LogInformation("Sending email to {RecipientEmail} by UserId={UserId}", request.RecipientEmail, userId);

    //         await accessorClient.SendEmailAsync(userId, request, ct);

    //         return Results.Ok(new { message = "Email sent successfully." });
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Failed to send email to {RecipientEmail}", request.RecipientEmail);
    //         return Results.Problem("Email sending failed.");
    //     }
    // }
}
