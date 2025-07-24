using Manager.Models;
using Manager.Services;

namespace Manager.Endpoints;

public static class ManagerEndpoints
{

    public static void MapManagerEndpoints(this WebApplication app)
    {


        #region HTTP GET

        app.MapGet("/task/{id:int}", async (int id,
            IManagerService managerService,
            ILogger<ManagerService> logger) =>
        {
            try
            {
                var task = await managerService.GetTaskAsync(id);
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

        #endregion


        #region HTTP POST


        app.MapPost("/task", async (TaskModel task,
            IManagerService managerService,
            ILogger<ManagerService> logger) =>
        {
            try
            {
                logger.LogDebug("Received task for processing: {Id} - {Name}", task.Id, task.Name);

                var (success, message) = await managerService.ProcessTaskAsync(task);

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

        #endregion


        #region HTTP PUT

        app.MapPut("/task/{id}/{name}", async (int id,
           string name, ILogger<ManagerService> logger,
           IManagerService managerService) =>
        {
            logger.LogInformation("Get account by id from account manager with {id}", id);
            try
            {
                var success = await managerService.UpdateTaskName(id, name);
                return success ? Results.Ok("Task name updated") : Results.NotFound("Task not found");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating task name for ID {Id}", id);
                return Results.Problem("An error occurred while updating the task name.");
            }       

        });

        #endregion


        #region HTTP DELETE


        app.MapDelete("/task/{id}", async (int id,
            ILogger<ManagerService> logger,
            IManagerService managerService) =>
        {
            logger.LogInformation("Delete task with id {id}", id);
            try
            {
                var success = await managerService.DeleteTask(id);
                return success ? Results.Ok("Task deleted") : Results.NotFound("Task not found");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting task with ID {Id}", id);
                return Results.Problem("An error occurred while deleting the task.");
            }

        });


        #endregion


    }
}
