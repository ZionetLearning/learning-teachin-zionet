using Manager.Models;
using Manager.Services;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class ManagerEndpoints
{
    public static WebApplication MapManagerEndpoints(this WebApplication app)
    {


        #region HTTP GET

        app.MapGet("/task/{id}", GetTaskAsync).WithName("GetTask");

        #endregion


        #region HTTP POST

        app.MapPost("/task", CreateTaskAsync).WithName("CreateTask");

        #endregion


        #region HTTP PUT

        app.MapPut("/task/{id}/{name}", UpdateTaskNameAsync).WithName("UpdateTaskName");

        #endregion


        #region HTTP DELETE

        app.MapDelete("/task/{id}", DeleteTaskAsync).WithName("DeleteTask");

        #endregion

        return app;
    }

    private static async Task<IResult> GetTaskAsync(
        [FromRoute] int id,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        logger.LogInformation("Inside {method}", nameof(GetTaskAsync));
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
    }

    private static async Task<IResult> CreateTaskAsync(
        [FromBody] TaskModel task,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        logger.LogInformation("Inside {method}", nameof(CreateTaskAsync));
        try
        {
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
            logger.LogError(ex, "Error processing task {Id}", task.Id);
            return Results.Problem("An error occurred while processing the task.");
        }
    }

    private static async Task<IResult> UpdateTaskNameAsync(
        [FromRoute] int id,
        [FromRoute] string name,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        logger.LogInformation("Inside {method}", nameof(UpdateTaskNameAsync));
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
    }

    private static async Task<IResult> DeleteTaskAsync(
        [FromRoute] int id,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        logger.LogInformation("Inside {method}", nameof(DeleteTaskAsync));
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
    }

}