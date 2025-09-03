using Manager.Models;
using Manager.Models.ModelValidation;
using Manager.Services.Clients.Accessor;
using Manager.Services.Clients.Engine;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class TasksEndpoints
{
    private sealed class TaskEndpoint { }

    public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        var tasksGroup = app.MapGroup("/tasks-manager").WithTags("Tasks").RequireAuthorization("AdminOrTeacherOrStudent");

        tasksGroup.MapGet("/task/{id:int}", GetTaskAsync).WithName("GetTask").RequireAuthorization("AdminOrTeacherOrStudent");
        tasksGroup.MapPost("/task", CreateTaskAsync).WithName("CreateTask").RequireAuthorization("AdminOrTeacher");
        tasksGroup.MapPost("/tasklong", CreateTaskLongAsync).WithName("CreateTaskLongTest").RequireAuthorization("AdminOrTeacher");
        tasksGroup.MapPut("/task/{id:int}/{name}", UpdateTaskNameAsync).WithName("UpdateTaskName").RequireAuthorization("AdminOrTeacher");
        tasksGroup.MapDelete("/task/{id:int}", DeleteTaskAsync).WithName("DeleteTask").RequireAuthorization("AdminOrTeacher");

        return app;
    }

    private static async Task<IResult> GetTaskAsync(
        [FromRoute] int id,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<TaskEndpoint> logger)
    {
        using var scope = logger.BeginScope("TaskId {TaskId}:", id);

        if (id <= 0)
        {
            logger.LogWarning("Invalid task ID");
            return Results.NotFound(new { Message = $"Task with ID {id} not found" });
        }

        try
        {
            var task = await accessorClient.GetTaskAsync(id);
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

    private static async Task<IResult> CreateTaskAsync(
        [FromBody] TaskModel task,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<TaskEndpoint> logger)
    {
        using var scope = logger.BeginScope("TaskId {TaskId}:", task.Id);

        if (!ValidationExtensions.TryValidate(task, out var validationErrors))
        {
            logger.LogWarning("Validation failed for {Model}: {Errors}", nameof(TaskModel), validationErrors);
            return Results.BadRequest(new { errors = validationErrors });
        }

        if (string.IsNullOrWhiteSpace(task.Name))
        {
            logger.LogWarning("Task {TaskId} has invalid name", task.Id);
            return Results.BadRequest("Task name is required");
        }

        if (string.IsNullOrWhiteSpace(task.Payload))
        {
            logger.LogWarning("Task {TaskId} has invalid payload", task.Id);
            return Results.BadRequest("Task payload is required");
        }

        try
        {
            logger.LogInformation("Posting task {TaskId} with name '{TaskName}'", task.Id, task.Name);
            var result = await accessorClient.PostTaskAsync(task);

            if (result.success)
            {
                logger.LogInformation("Task {TaskId} successfully posted", task.Id);
                return Results.Accepted($"/tasks-manager/task/{task.Id}", new { status = result.message, task.Id });
            }

            logger.LogWarning("Task {TaskId} failed: {Message}", task.Id, result.message);
            return Results.Problem(result.message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing task");
            return Results.Problem("An error occurred while processing the task.");
        }
    }

    private static async Task<IResult> CreateTaskLongAsync(
        [FromBody] TaskModel task,
        [FromServices] IEngineClient engineClient,
        [FromServices] ILogger<TaskEndpoint> logger)
    {
        using var scope = logger.BeginScope("TaskId {TaskId}:", task.Id);

        try
        {
            logger.LogInformation("Long running flow test");
            var result = await engineClient.ProcessTaskLongAsync(task);

            return result.success
                ? Results.Accepted("Long running task accepted")
                : Results.Problem(result.message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing task");
            return Results.Problem("An error occurred while processing the task.");
        }
    }

    private static async Task<IResult> UpdateTaskNameAsync(
        [FromRoute] int id,
        [FromRoute] string name,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<TaskEndpoint> logger)
    {
        using var scope = logger.BeginScope("TaskId {TaskId}:", id);

        if (id <= 0)
        {
            logger.LogWarning("Invalid task ID");
            return Results.NotFound(new { Message = $"Task with ID {id} not found" });
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            logger.LogWarning("Invalid task name");
            return Results.BadRequest("Invalid task name");
        }

        if (name.Length > 100)
        {
            logger.LogWarning("Task name too long");
            return Results.BadRequest("Task name too long");
        }

        try
        {
            logger.LogInformation("Attempting to update task name");
            var success = await accessorClient.UpdateTaskName(id, name);

            if (success)
            {
                logger.LogInformation("Successfully updated task name");
                return Results.Ok("Task name updated");
            }

            logger.LogWarning("Task not found for update");
            return Results.NotFound("Task not found");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating task name");
            return Results.Problem("An error occurred while updating the task name.");
        }
    }

    private static async Task<IResult> DeleteTaskAsync(
        [FromRoute] int id,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<TaskEndpoint> logger)
    {
        using var scope = logger.BeginScope("TaskId {TaskId}:", id);

        if (id <= 0)
        {
            logger.LogWarning("Invalid task ID");
            return Results.NotFound(new { Message = $"Task with ID {id} not found" });
        }

        try
        {
            logger.LogInformation("Attempting to delete task");
            var success = await accessorClient.DeleteTask(id);

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