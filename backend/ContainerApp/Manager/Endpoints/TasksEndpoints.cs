using Manager.Constants;
using Manager.Mapping;
using Manager.Models;
using Manager.Models.ModelValidation;
using Manager.Models.Tasks.Requests;
using Manager.Models.Tasks.Responses;
using Manager.Services.Clients.Accessor.Interfaces;
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
        [FromServices] ITaskAccessorClient taskAccessorClient,
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
            var (accessorTask, etag) = await taskAccessorClient.GetTaskWithEtagAsync(id);

            if (accessorTask is not null)
            {
                if (!string.IsNullOrWhiteSpace(etag))
                {
                    response.Headers.ETag = $"\"{etag}\"";
                }

                var taskResponse = accessorTask.ToFront();
                logger.LogInformation("Successfully retrieved task (ETag forwarded).");
                return Results.Ok(taskResponse);
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
        [FromBody] CreateTaskRequest request,
        [FromServices] ITaskAccessorClient taskAccessorClient,
        [FromServices] ILogger<TaskEndpoint> logger)
    {
        using var scope = logger.BeginScope("TaskId {TaskId}:", request.Id);

        if (!ValidationExtensions.TryValidate(request, out var validationErrors))
        {
            logger.LogWarning("Validation failed for {Model}: {Errors}", nameof(CreateTaskRequest), validationErrors);
            return Results.BadRequest(new { errors = validationErrors });
        }

        try
        {
            logger.LogInformation("Posting task {TaskId} with name '{TaskName}'", request.Id, request.Name);
            var accessorRequest = request.ToAccessor();
            var accessorResult = await taskAccessorClient.PostTaskAsync(accessorRequest);

            if (accessorResult.Success)
            {
                var response = accessorResult.ToFront();
                logger.LogInformation("Task {TaskId} successfully posted", request.Id);
                return Results.Accepted($"/tasks-manager/task/{request.Id}", response);
            }

            logger.LogWarning("Task {TaskId} failed: {Message}", request.Id, accessorResult.Message);
            return Results.Problem(accessorResult.Message);
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
        [FromServices] ITaskAccessorClient taskAccessorClient,
        [FromServices] ILogger<TaskEndpoint> logger)
    {
        using var scope = logger.BeginScope("List all tasks");
        try
        {
            var accessorResult = await taskAccessorClient.GetTasksAsync();
            var response = accessorResult.ToFront();
            logger.LogInformation("Retrieved {Count} tasks", response.Count);
            return Results.Ok(response);
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
        [FromServices] ITaskAccessorClient taskAccessorClient,
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
            logger.LogWarning("Task name is required");
            return Results.BadRequest(new { Message = "Task name is required" });
        }

        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            logger.LogWarning("Missing If-Match header");
            return Results.StatusCode(StatusCodes.Status428PreconditionRequired);
        }

        try
        {
            logger.LogInformation("Attempting to update task name to '{Name}'", name);
            var accessorResult = await taskAccessorClient.UpdateTaskNameAsync(id, name, ifMatch!);

            if (accessorResult.NotFound)
            {
                logger.LogWarning("Task not found for update");
                return Results.NotFound("Task not found");
            }

            if (accessorResult.PreconditionFailed)
            {
                logger.LogWarning("Precondition failed (ETag mismatch)");
                return Results.StatusCode(StatusCodes.Status412PreconditionFailed);
            }

            if (!string.IsNullOrWhiteSpace(accessorResult.NewEtag))
            {
                response.Headers.ETag = $"\"{accessorResult.NewEtag}\"";
            }

            var updateResponse = accessorResult.ToFront();
            logger.LogInformation("Successfully updated task name");
            return Results.Ok(updateResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating task name");
            return Results.Problem("An error occurred while updating the task name.");
        }
    }

    private static async Task<IResult> DeleteTaskAsync(
        [FromRoute] int id,
        [FromServices] ITaskAccessorClient taskAccessorClient,
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
            var success = await taskAccessorClient.DeleteTask(id);

            if (success)
            {
                logger.LogInformation("Successfully deleted task");
                var response = new DeleteTaskResponse { Message = "Task deleted" };
                return Results.Ok(response);
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