using Manager.Constants;
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
        var tasksGroup = app.MapGroup("/tasks-manager").WithTags("Tasks").RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        tasksGroup.MapGet("/task/{id:int}", GetTaskAsync).WithName("GetTask").RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);
        tasksGroup.MapGet("/tasks", GetTasksAsync).WithName("GetTasks").RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);
        tasksGroup.MapPost("/task", CreateTaskAsync).WithName("CreateTask").RequireAuthorization(PolicyNames.AdminOrTeacher);
        tasksGroup.MapPost("/tasklong", CreateTaskLongAsync).WithName("CreateTaskLongTest").RequireAuthorization(PolicyNames.AdminOrTeacher);
        tasksGroup.MapPut("/task/{id:int}/{name}", UpdateTaskNameAsync).WithName("UpdateTaskName").RequireAuthorization(PolicyNames.AdminOrTeacher);
        tasksGroup.MapDelete("/task/{id:int}", DeleteTaskAsync).WithName("DeleteTask").RequireAuthorization(PolicyNames.AdminOrTeacher);

        return app;
    }

    private static async Task<IResult> GetTaskAsync(
        [FromRoute] int id,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<TaskEndpoint> logger,
        HttpResponse response)
    {
        using var scope = logger.BeginScope("TaskId {TaskId}:", id);

        if (id <= 0)
        {
            logger.LogWarning("Invalid task ID");
            return Results.NotFound(new { Message = $"Task with ID {id} not found" });
        }

        try
        {
            var (task, etag) = await accessorClient.GetTaskWithEtagAsync(id);

            if (task is not null)
            {
                if (!string.IsNullOrWhiteSpace(etag))
                {
                    response.Headers.ETag = $"\"{etag}\"";
                }

                logger.LogInformation("Successfully retrieved task (ETag forwarded).");
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

    private static async Task<IResult> GetTasksAsync(
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<TaskEndpoint> logger)
    {
        using var scope = logger.BeginScope("List all tasks");
        try
        {
            var items = await accessorClient.GetTaskSummariesAsync();
            logger.LogInformation("Retrieved {Count} tasks", items?.Count ?? 0);
            return Results.Ok(items);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving tasks list");
            return Results.Problem("An error occurred while retrieving tasks.");
        }
    }
    private static async Task<IResult> UpdateTaskNameAsync(
        [FromRoute] int id,
        [FromRoute] string name,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<TaskEndpoint> logger,
        HttpResponse response)
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

        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            logger.LogWarning("Missing If-Match header");
            return Results.StatusCode(StatusCodes.Status428PreconditionRequired);
        }

        try
        {
            logger.LogInformation("Attempting to update task name");
            var result = await accessorClient.UpdateTaskNameAsync(id, name, ifMatch!);

            if (result.NotFound)
            {
                logger.LogWarning("Task not found for update");
                return Results.NotFound("Task not found");
            }

            if (result.PreconditionFailed)
            {
                logger.LogWarning("Precondition failed (ETag mismatch)");
                return Results.StatusCode(StatusCodes.Status412PreconditionFailed);
            }

            if (!string.IsNullOrWhiteSpace(result.NewEtag))
            {
                response.Headers.ETag = $"\"{result.NewEtag}\"";
            }

            logger.LogInformation("Successfully updated task name");
            return Results.Ok("Task name updated");
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