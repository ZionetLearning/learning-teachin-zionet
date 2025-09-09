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
        promptGroup.MapGet("/{promptKey}/versions/{version}", GetPromptVersionAsync).WithName("GetPromptVersion");
        promptGroup.MapPost("/batch", GetPromptsBatchAsync).WithName("GetPromptsBatch");

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

    public static async Task<IResult> GetPromptsBatchAsync(
        [FromBody] GetPromptsBatchRequest request,
        [FromServices] IPromptService promptService,
        [FromServices] ILogger<PromptService> logger,
        CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope("Method: {Method}", nameof(GetPromptsBatchAsync));

        try
        {
            if (request is null || request.PromptKeys is null || request.PromptKeys.Count == 0)
            {
                logger.LogWarning("Batch request missing prompt keys");
                return Results.BadRequest(new { error = "PromptKeys are required." });
            }

            const int maxBatch = 100;
            if (request.PromptKeys.Count > maxBatch)
            {
                return Results.BadRequest(new { error = $"Maximum {maxBatch} prompt keys allowed." });
            }

            var prompts = await promptService.GetLatestPromptsAsync(request.PromptKeys, cancellationToken);
            var foundSet = prompts.Select(p => p.PromptKey).ToHashSet(StringComparer.Ordinal);
            var notFound = request.PromptKeys
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Where(k => !foundSet.Contains(k))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            logger.LogInformation("Batch retrieval complete. Found {Found} Missing {Missing}", prompts.Count, notFound.Count);

            return Results.Ok(new GetPromptsBatchResponse
            {
                Prompts = prompts,
                NotFound = notFound
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Validation failure in batch retrieval");
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in batch retrieval");
            return Results.Problem("Failed to retrieve prompts batch.");
        }
    }

    public static async Task<IResult> GetPromptVersionAsync(
        [FromRoute] string promptKey,
        [FromRoute] string version,
        [FromServices] IPromptService promptService,
        [FromServices] ILogger<PromptService> logger,
        CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope("Method: {Method}, PromptKey: {PromptKey}, Version: {Version}", nameof(GetPromptVersionAsync), promptKey, version);

        try
        {
            if (string.IsNullOrWhiteSpace(promptKey) || string.IsNullOrWhiteSpace(version))
            {
                logger.LogWarning("Invalid request for specific prompt version.");
                return Results.BadRequest(new { error = "PromptKey and Version are required." });
            }

            var prompt = await promptService.GetPromptByVersionAsync(promptKey, version, cancellationToken);
            if (prompt is null)
            {
                logger.LogInformation("Prompt {PromptKey} version {Version} not found.", promptKey, version);
                return Results.NotFound(new { error = $"Prompt '{promptKey}' with version '{version}' not found." });
            }

            logger.LogInformation("Retrieved prompt {PromptKey} version {Version}", promptKey, version);
            return Results.Ok(prompt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving prompt {PromptKey} version {Version}", promptKey, version);
            return Results.Problem("Failed to retrieve prompt version.");
        }
    }
}