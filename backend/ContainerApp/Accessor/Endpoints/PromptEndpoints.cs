using Accessor.Models.Prompts;
using Accessor.Services;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class PromptEndpoints
{
    public static IEndpointRouteBuilder MapPromptEndpoints(this IEndpointRouteBuilder app)
    {
        var promptGroup = app.MapGroup("/prompts").WithTags("Prompts");

        promptGroup.MapPost("/", CreatePromptAsync).WithName("CreatePrompt");
        promptGroup.MapGet("/{promptKey}", GetPromptAsync).WithName("GetPrompt");
        promptGroup.MapGet("/{promptKey}/versions", GetPromptVersionsAsync).WithName("GetPromptVersions");

        return app;
    }

    public static async Task<IResult> CreatePromptAsync(
        [FromBody] CreatePromptRequest request,
        [FromServices] IPromptService promptService,
        [FromServices] ILogger<PromptService> logger,
        CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope("Method: {Method}, PromptKey: {PromptKey}", nameof(CreatePromptAsync), request?.PromptKey);

        try
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.PromptKey) ||
                string.IsNullOrWhiteSpace(request.Content))
            {
                logger.LogWarning("Invalid create prompt request.");
                return Results.BadRequest(new { error = "PromptKey and Content are required." });
            }

            var result = await promptService.CreatePromptAsync(request, cancellationToken);
            logger.LogInformation("Created prompt {PromptKey}", result.PromptKey);

            return Results.CreatedAtRoute("GetPrompt", new { promptKey = result.PromptKey }, result);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Validation failure creating prompt.");
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating prompt.");
            return Results.Problem("An unexpected error occurred while creating the prompt.");
        }
    }

    public static async Task<IResult> GetPromptAsync(
        [FromRoute] string promptKey,
        [FromServices] IPromptService promptService,
        [FromServices] ILogger<PromptService> logger,
        CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope("Method: {Method}, PromptKey: {PromptKey}", nameof(GetPromptAsync), promptKey);

        try
        {
            var prompt = await promptService.GetLatestPromptAsync(promptKey, cancellationToken);
            if (prompt is null)
            {
                logger.LogInformation("Prompt {PromptKey} not found.", promptKey);
                return Results.NotFound(new { error = $"Prompt with key '{promptKey}' not found." });
            }

            logger.LogInformation("Retrieved latest prompt {PromptKey}", prompt.PromptKey);
            return Results.Ok(prompt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving prompt {PromptKey}", promptKey);
            return Results.Problem("Failed to retrieve prompt.");
        }
    }

    public static async Task<IResult> GetPromptVersionsAsync(
        [FromRoute] string promptKey,
        [FromServices] IPromptService promptService,
        [FromServices] ILogger<PromptService> logger,
        CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope("Method: {Method}, PromptKey: {PromptKey}", nameof(GetPromptVersionsAsync), promptKey);

        try
        {
            var versions = await promptService.GetAllVersionsAsync(promptKey, cancellationToken);
            logger.LogInformation("Retrieved {Count} versions for prompt {PromptKey}", versions.Count, promptKey);
            return Results.Ok(versions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving versions for prompt {PromptKey}", promptKey);
            return Results.Problem("Failed to retrieve prompt versions.");
        }
    }
}