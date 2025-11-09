using Accessor.Models.Prompts;
using Accessor.Services;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class PromptEndpoints
{
    public static IEndpointRouteBuilder MapPromptEndpoints(this IEndpointRouteBuilder app)
    {
        var promptGroup = app.MapGroup("/prompts").WithTags("Prompts");

        promptGroup.MapPost("/", CreatePromptAsync).WithName("CreatePrompt");
        promptGroup.MapGet("/", GetAllPromptsAsync).WithName("GetAllPrompts");
        promptGroup.MapGet("/{promptKey}", GetPromptAsync).WithName("GetPrompt");
        promptGroup.MapPost("/batch", GetPromptsBatchAsync).WithName("GetPromptsBatch");
        promptGroup.MapPatch("/{promptKey}/versions/{version}/labels", UpdatePromptLabelsAsync).WithName("UpdatePromptLabels");

        return app;
    }

    public static async Task<IResult> CreatePromptAsync(
        [FromBody] CreatePromptRequest request,
        [FromServices] IPromptService promptService,
        [FromServices] ILogger<PromptService> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Creating new prompt with key {PromptKey}", request?.PromptKey);
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

    public static async Task<IResult> GetAllPromptsAsync(
        [FromServices] IPromptService promptService,
        [FromServices] ILogger<PromptService> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving all prompts");
            var prompts = await promptService.GetAllPromptsAsync(cancellationToken);
            logger.LogInformation("Retrieved {Count} prompts", prompts.Count);
            return Results.Ok(prompts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all prompts");
            return Results.Problem("Failed to retrieve prompts.");
        }
    }

    public static async Task<IResult> GetPromptAsync(
        [FromRoute] string promptKey,
        [FromQuery] int? version,
        [FromQuery] string? label,
        [FromServices] IPromptService promptService,
        [FromServices] ILogger<PromptService> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving prompt {PromptKey} with version {Version} and label {Label}", promptKey, version, label);
            var prompt = await promptService.GetPromptAsync(promptKey, version, label, cancellationToken);
            if (prompt is null)
            {
                logger.LogInformation("Prompt {PromptKey} not found.", promptKey);
                return Results.NotFound(new { error = $"Prompt with key '{promptKey}' not found." });
            }

            logger.LogInformation("Retrieved prompt {PromptKey} from {Source}", prompt.PromptKey, prompt.Source);
            return Results.Ok(prompt);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid argument for prompt {PromptKey}", promptKey);
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving prompt {PromptKey}", promptKey);
            return Results.Problem("Failed to retrieve prompt.");
        }
    }

    public static async Task<IResult> GetPromptsBatchAsync(
        [FromBody] GetPromptsBatchRequest request,
        [FromServices] IPromptService promptService,
        [FromServices] ILogger<PromptService> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Starting batch retrieval of prompts. Requested count: {Count}", request?.Prompts?.Count ?? 0);
            if (request is null || request.Prompts is null || request.Prompts.Count == 0)
            {
                logger.LogWarning("Batch request missing prompt configurations");
                return Results.BadRequest(new { error = "Prompts are required." });
            }

            const int maxBatch = 100;
            if (request.Prompts.Count > maxBatch)
            {
                return Results.BadRequest(new { error = $"Maximum {maxBatch} prompt configurations allowed." });
            }

            var results = new List<PromptResponse>();
            var notFound = new List<string>();

            foreach (var config in request.Prompts)
            {
                try
                {
                    var prompt = await promptService.GetPromptAsync(config.Key, config.Version, config.Label, cancellationToken);
                    if (prompt != null)
                    {
                        results.Add(prompt);
                    }
                    else
                    {
                        notFound.Add(config.Key);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to get prompt {PromptKey} in batch", config.Key);
                    notFound.Add(config.Key);
                }
            }

            logger.LogInformation("Batch retrieval complete. Found {Found} Missing {Missing}", results.Count, notFound.Count);

            return Results.Ok(new GetPromptsBatchResponse
            {
                Prompts = results,
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

    public static async Task<IResult> UpdatePromptLabelsAsync(
        [FromRoute] string promptKey,
        [FromRoute] int version,
        [FromBody] UpdatePromptLabelsRequest request,
        [FromServices] IPromptService promptService,
        [FromServices] ILogger<PromptService> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Updating labels for prompt {PromptKey} version {Version}", promptKey, version);
            if (string.IsNullOrWhiteSpace(promptKey))
            {
                return Results.BadRequest(new { error = "PromptKey is required." });
            }

            if (request is null || request.NewLabels is null || request.NewLabels.Length == 0)
            {
                return Results.BadRequest(new { error = "NewLabels are required." });
            }

            var updated = await promptService.UpdatePromptLabelsAsync(promptKey, version, request, cancellationToken);
            logger.LogInformation("Updated labels for prompt {PromptKey} version {Version}", promptKey, version);

            return Results.Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Prompt not found");
            return Results.NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Validation failure");
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating prompt labels");
            return Results.Problem("Failed to update prompt labels.");
        }
    }
}