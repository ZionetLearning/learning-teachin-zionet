using Engine.Constants;
using Engine.Models;
using Engine.Services;
using Microsoft.AspNetCore.Mvc;

namespace Engine.Endpoints;

public static class EngineEndpoints
{
    public static WebApplication MapEngineEndpoints(this WebApplication app)
    {

        #region HTTP POST

        app.MapPost($"/{QueueNames.ManagerToEngine}-input", ProcessTaskAsync).WithName("ProcessTask");

        #endregion

        return app;
    }

    private static async Task<IResult> ProcessTaskAsync(
            [FromBody] TaskModel task,
            [FromServices] IEngineService engineService,
            [FromServices] ILogger<EngineService> logger)
    {
        logger.LogInformation("Inside {Method}", nameof(ProcessTaskAsync));
        try
        {
            logger.LogDebug("Received task from queue input: {Id} - {Name}", task.Id, task.Name);
            await engineService.ProcessTaskAsync(task);
            logger.LogInformation("Task {Id} processed successfully", task.Id);
            return Results.Ok(new { Status = "Forwarded to accessor", task.Id });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process task {Id}", task.Id);
            return Results.Problem("An error occurred while processing the task.");
        }
    }
}
