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
        logger.LogInformation("Inside {Method}", nameof(GetTaskAsync));
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
        HttpRequest request,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        logger.LogInformation("Inside {Method}", nameof(CreateTaskAsync));

        // 1. Validate task model
        if (!ValidationExtensions.TryValidate(task, out var validationErrors))
        {
            logger.LogWarning("Validation failed for {Model}: {Errors}", nameof(TaskModel), validationErrors);
            return Results.BadRequest(new { errors = validationErrors });
        }

        // 2. Check idempotency key in headers
        if (!request.Headers.TryGetValue("X-Request-ID", out var requestId) || string.IsNullOrWhiteSpace(requestId))
        {
            logger.LogWarning("Missing or invalid X-Request-ID header");
            return Results.BadRequest(new { error = "Missing X-Request-ID header" });
        }

        // 3. Call manager service with idempotency
        try
        {
            var (success, message, isDuplicate) = await managerService.ProcessTaskWithIdempotencyAsync(task, requestId!);

            if (isDuplicate)
            {
                logger.LogInformation("Duplicate request detected for {RequestId}", requestId.ToString());
                return Results.Accepted($"/task/{task.Id}", new { status = "AlreadyProcessed", task.Id });
            }

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
        logger.LogInformation("Inside {Method}", nameof(UpdateTaskNameAsync));
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
        logger.LogInformation("Inside {Method}", nameof(DeleteTaskAsync));
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