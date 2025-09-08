﻿using Accessor.Models;
using Accessor.Services;
using Accessor.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class TasksEndpoints
{
    public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        var tasksGroup = app.MapGroup("/tasks-accessor").WithTags("Tasks");

        tasksGroup.MapGet("/task/{id:int}", GetTaskByIdAsync).WithName("GetTaskById");
        tasksGroup.MapPost("/task", CreateTaskAsync).WithName("CreateTask");
        tasksGroup.MapDelete("/task/{taskId:int}", DeleteTaskAsync).WithName("DeleteTask");

        return app;
    }

    // ---- handlers (moved from AccessorEndpoints) ----
    public static async Task<IResult> GetTaskByIdAsync(
        int id,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger)
    {
        using var scope = logger.BeginScope("Method: {Method}, TaskId: {TaskId}", nameof(GetTaskByIdAsync), id);
        try
        {
            var task = await accessorService.GetTaskByIdAsync(id);
            if (task != null)
            {
                logger.LogInformation("Successfully retrieved task.");
                return Results.Ok(task);
            }

            logger.LogWarning("Task not found.");
            return Results.NotFound($"Task with ID {id} not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error while retrieving task.");
            return Results.Problem("An error occurred while fetching the task.");
        }
    }

    public static async Task<IResult> CreateTaskAsync(
        [FromBody] TaskModel task,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("Method: {Method}, TaskId: {TaskId}", nameof(CreateTaskAsync), task.Id);

        try
        {
            await accessorService.CreateTaskAsync(task);
            return Results.Created($"/api/tasks/{task.Id}", task);
        }
        catch (ConflictException ex)
        {
            return Results.Conflict(new { error = ex.Message }); // 409
        }
        catch (NonRetryableException ex)
        {
            return Results.UnprocessableEntity(new { error = ex.Message }); // 422
        }
        catch (RetryableException ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable); // 503
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating Task {TaskId}", task.Id);
            return Results.Problem("An unexpected error occurred while saving the task."); // 500
        }
    }

    public static async Task<IResult> UpdateTaskNameAsync(
    [FromBody] UpdateTaskName request,
    [FromServices] IAccessorService accessorService,
    [FromServices] ILogger<AccessorService> logger)
    {
        using var scope = logger.BeginScope("Method: {Method}, TaskId: {TaskId}, NewName: {NewName}", nameof(UpdateTaskNameAsync), request.Id, request.Name);
        try
        {
            var success = await accessorService.UpdateTaskNameAsync(request.Id, request.Name);
            if (!success)
            {
                logger.LogWarning("Task not found.");
                return Results.NotFound($"Task with ID {request.Id} not found.");
            }

            logger.LogInformation("Task updated successfully.");
            return Results.Ok($"Task {request.Id} updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update task.");
            return Results.Problem("Internal server error while updating task.");
        }
    }

    public static async Task<IResult> DeleteTaskAsync(
        int taskId,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger)
    {
        using var scope = logger.BeginScope("Method: {Method}, TaskId: {TaskId}", nameof(DeleteTaskAsync), taskId);
        try
        {
            var deleted = await accessorService.DeleteTaskAsync(taskId);
            if (!deleted)
            {
                logger.LogWarning("Task not found.");
                return Results.NotFound($"Task with ID {taskId} not found.");
            }

            logger.LogInformation("Task deleted successfully.");
            return Results.Ok($"Task {taskId} deleted.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete task.");
            return Results.Problem("Internal server error while deleting the task.");
        }
    }
}
