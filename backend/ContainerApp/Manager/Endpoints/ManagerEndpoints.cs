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
        using (logger.BeginScope("Method {MethodName}:", nameof(GetTaskAsync)))
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
    HttpRequest request,
    [FromServices] IManagerService managerService,
    [FromServices] ILogger<ManagerService> logger)
    {
        using (logger.BeginScope("Method: {Method}", nameof(CreateTaskAsync)))
        {
            if (!ValidationExtensions.TryValidate(task, out var validationErrors))
            {
                logger.LogWarning("Validation failed for {Model}: {Errors}", nameof(TaskModel), validationErrors);
                return Results.BadRequest(new { errors = validationErrors });
            }

            // Extract Idempotency-Key from headers
            if (!request.Headers.TryGetValue("Idempotency-Key", out var requestId) || string.IsNullOrWhiteSpace(requestId))
            {
                logger.LogWarning("Missing or invalid Idempotency-Key header");
                return Results.BadRequest(new { error = "Missing Idempotency-Key header" });
            }

            try
            {
                logger.LogInformation("Processing task creation for ID {TaskId}", task.Id);

                var (success, message, taskId) = await managerService.ProcessTaskAsync(task, requestId!);

                if (success)
                {
                    logger.LogInformation("Task {TaskId} processed successfully", task.Id);
                    return Results.Accepted($"/task/{task.Id}", new { status = message, task.Id });
                }

                logger.LogWarning("Task {TaskId} was already processed", task.Id);
                return Results.Conflict(new { status = message, id = taskId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing task {TaskId}", task.Id);
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
        using (logger.BeginScope("Method: {Method}", nameof(UpdateTaskNameAsync)))
        {
            try
            {
                logger.LogInformation("Attempting to update task name for ID {TaskId}", id);

                var success = await managerService.UpdateTaskName(id, name);

                if (success)
                {
                    logger.LogInformation("Successfully updated task name for ID {TaskId}", id);
                    return Results.Ok("Task name updated");
                }

                logger.LogWarning("Task with ID {TaskId} not found for name update", id);
                return Results.NotFound("Task not found");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating task name for ID {TaskId}", id);
                return Results.Problem("An error occurred while updating the task name.");
            }
        }
    }

    private static async Task<IResult> DeleteTaskAsync(
        [FromRoute] int id,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using (logger.BeginScope("Method: {Method}", nameof(DeleteTaskAsync)))
        {
            try
            {
                logger.LogInformation("Attempting to delete task with ID {TaskId}", id);

                var success = await managerService.DeleteTask(id);

                if (success)
                {
                    logger.LogInformation("Successfully deleted task with ID {TaskId}", id);
                    return Results.Ok("Task deleted");
                }

                logger.LogWarning("Task with ID {TaskId} not found for deletion", id);
                return Results.NotFound(new { Message = $"Task with ID {id} not found" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting task with ID {TaskId}", id);
                return Results.Problem("An error occurred while deleting the task.");
            }
        }
    }
}