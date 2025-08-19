using Manager.Models;
using Manager.Models.ModelValidation;
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

        app.MapPost("/task", CreateTaskAsync).WithName("CreateTaskAsync");

        app.MapPost("/tasklong", CreateTaskLongAsync).WithName("CreateTaskLongTest");
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
        using var scope = logger.BeginScope("TaskId {TaskId}:", id);
        {
            try
            {
                var task = await managerService.GetTaskAsync(id);
                if (task is not null)
                {
                    logger.LogInformation("Successfully retrieved task");
                    return Results.Ok(task);
                }

                logger.LogWarning("Task not found");
                return Results.NotFound(new { Message = $"Task with ID {id} not found" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while retrieving task");
                return Results.Problem("An error occurred while retrieving the task.");
            }
        }
    }

    private static async Task<IResult> CreateTaskAsync(
        [FromBody] TaskModel task,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using var scope = logger.BeginScope("TaskId {TaskId}:", task.Id);
        {
            if (!ValidationExtensions.TryValidate(task, out var validationErrors))
            {
                logger.LogWarning("Validation failed for {Model}: {Errors}", nameof(TaskModel), validationErrors);
                return Results.BadRequest(new { errors = validationErrors });
            }

            try
            {
                logger.LogInformation("Processing task creation");

                var (success, message) = await managerService.CreateTaskAsync(task);
                if (success)
                {
                    logger.LogInformation("Task sent to queue successfully");
                    return Results.Accepted($"/task/{task.Id}", new { status = message, task.Id });
                }

                logger.LogWarning("Processing task failed: {Message}", message);
                return Results.Problem("Failed to process the task.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing task");
                return Results.Problem("An error occurred while processing the task.");
            }
        }
    }

    private static async Task<IResult> CreateTaskLongAsync(
        [FromBody] TaskModel task,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using var scope = logger.BeginScope("TaskId {TaskId}:", task.Id);
        {
            try
            {
                logger.LogInformation("Long running flow test");
                await managerService.ProcessTaskLongAsync(task);
                return Results.Accepted("Long running task accepted");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing task");
                return Results.Problem("An error occurred while processing the task.");
            }
        }
    }

    private static async Task<IResult> UpdateTaskNameAsync(
        [FromRoute] int id,
        [FromRoute] string name,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using var scope = logger.BeginScope("TaskId {TaskId}:", id);
        {
            try
            {
                logger.LogInformation("Attempting to update task name");

                var success = await managerService.UpdateTaskName(id, name);

                if (success)
                {
                    logger.LogInformation("Successfully updated task name");
                    return Results.Ok("Task name updated");
                }

                logger.LogWarning("Task not found for name update");
                return Results.NotFound("Task not found");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating task name ");
                return Results.Problem("An error occurred while updating the task name.");
            }
        }
    }

    private static async Task<IResult> DeleteTaskAsync(
        [FromRoute] int id,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using var scope = logger.BeginScope("TaskId {TaskId}:", id);
        {
            try
            {
                logger.LogInformation("Attempting to delete task ");

                var success = await managerService.DeleteTask(id);

                if (success)
                {
                    logger.LogInformation("Successfully deleted task");
                    return Results.Ok("Task deleted");
                }

                logger.LogWarning("Task not found for deletion");
                return Results.NotFound(new { Message = $"Task with ID {id} not found" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting task");
                return Results.Problem("An error occurred while deleting the task.");
            }
        }
    }
}