using Accessor.Constants;
using Accessor.Models;
using Accessor.Services;

namespace Accessor.Endpoints
{
    public static class AccessorEndpoints
    {
        public static void MapAccessorEndpoints(this WebApplication app)
        {
            app.MapGet("/task/{id:int}", async (int id, IAccessorService service, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("AccessorEndpoints");
                try
                {
                    var task = await service.GetTaskByIdAsync(id);
                    if (task is not null)
                    {
                        logger.LogInformation("Successfully retrieved task {Id}", task.Id);
                        return Results.Ok(task);
                    }

                    logger.LogWarning("Task with ID {Id} not found", id);
                    return Results.NotFound(new { Message = $"Task with ID {id} not found" });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unhandled error while retrieving task {Id}", id);
                    return Results.Problem("An error occurred while fetching the task.");
                }
            });

            app.MapPost($"/{QueueNames.EngineToAccessor}-input", async (TaskModel task, IAccessorService service, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("AccessorEndpoints");
                try
                {
                    await service.SaveTaskAsync(task);
                    logger.LogInformation("Task {Id} saved successfully", task.Id);
                    return Results.Ok(new { Status = "Saved", task.Id });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to save task {Id}", task.Id);
                    return Results.Problem("An error occurred while saving the task.");
                }
            });
        }
    }
}
