using Manager.Models;
using Manager.Services;

namespace Manager.Endpoints
{
    public static class ManagerEndpoints
    {
        private static readonly Dictionary<int, TaskModel> _tasks = new();

        public static void MapManagerEndpoints(this WebApplication app)
        {

            app.MapGet("/task/{id:int}", async (int id, IManagerService service, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("ManagerEndpoints");

                try
                {
                    var task = await service.GetTaskAsync(id);
                    if (task is not null)
                    {
                        logger.LogInformation("Retrieved task with ID {Id}", id);
                        return Results.Ok(task);
                    }

                    logger.LogWarning("Task with ID {Id} not found", id);
                    return Results.NotFound(new { error = $"Task with ID {id} not found." });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving task with ID {Id}", id);
                    return Results.Problem("An error occurred while retrieving the task.");
                }
            });

            app.MapPost("/task", async (TaskModel task, IManagerService service, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("ManagerEndpoints");

                try
                {
                    logger.LogDebug("Received task for processing: {Id} - {Name}", task.Id, task.Name);

                    var (success, message) = await service.ProcessTaskAsync(task);

                    if (success)
                    {
                        logger.LogInformation("Task {Id} processed successfully", task.Id);
                        return Results.Accepted($"/task/{task.Id}", new { status = message, task.Id });
                    }

                    logger.LogWarning("Processing task {Id} failed: {Message}", task.Id, message);
                    return Results.Problem("Failed to process the task.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Exception occurred while processing task {Id}", task.Id);
                    return Results.Problem("An internal error occurred while processing the task.");
                }
            });
        }
    }
}
