using Engine.Constants;
using Engine.Models;
using Engine.Services;

namespace Engine.Endpoints
{
public static class EngineEndpoints
{
    public static void MapEngineEndpoints(this WebApplication app)
    {
            
        app.MapPost($"/{QueueNames.ManagerToEngine}-input", 
            async (TaskModel task, IEngineService engineService, ILogger<EngineService> logger) =>
        {
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
        });


    }
}
}
