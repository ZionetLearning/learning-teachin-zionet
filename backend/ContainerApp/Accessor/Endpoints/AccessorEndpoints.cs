using Accessor.Constants;
using Accessor.Models;
using Accessor.Services;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class AccessorEndpoints
{
    public static void MapAccessorEndpoints(this WebApplication app)
    {
        app.MapGet("/task/{id:int}", GetTaskByIdAsync);
        app.MapPost($"/{QueueNames.EngineToAccessor}-input", CreateTaskAsync);
        app.MapPost($"/{QueueNames.TaskUpdateInput}", UpdateTaskNameAsync);
        app.MapDelete("/task/{taskId}", DeleteTaskAsync);

    }

    #region HandlerMethods
    public static async Task<IResult> GetTaskByIdAsync(
        int id,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger
    )
    {
        try
        {
            var task = await accessorService.GetTaskByIdAsync(id);
            if (task != null)
            {
                logger.LogInformation("Successfully retrieved task {Id}", task.Id);
                return Results.Ok(task);
            }

            logger.LogWarning("Task with ID {Id} not found", id);
            return Results.NotFound(new { Message = $"Task with ID {id} not found" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error while retrieving task {Id}", id);
            return Results.Problem("An error occurred while fetching the task.");
        }
    }

    public static async Task<IResult> CreateTaskAsync(
        TaskModel task,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger)
    {
        try
        {
            await accessorService.CreateTaskAsync(task);
            logger.LogInformation("Task {Id} saved successfully", task.Id);
            return Results.Ok(new { Status = "Saved", task.Id });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save task {Id}", task.Id);
            return Results.Problem("An error occurred while saving the task.");
        }
    }

    public static async Task<IResult> UpdateTaskNameAsync(
        [FromBody] UpdateTaskName request,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger)
    {
        logger.LogInformation("[Accessor] Received request to update task. Id: {Id}, NewName: {Name}", request.Id, request.Name);

        try
        {
            var success = await accessorService.UpdateTaskNameAsync(request.Id, request.Name);
            if (!success)
            {
                logger.LogWarning("[Accessor] Task with ID {Id} not found.", request.Id);
                return Results.NotFound($"Task with ID {request.Id} not found.");
            }

            logger.LogInformation("[Accessor] Task {Id} updated successfully.", request.Id);
            return Results.Ok($"Task {request.Id} updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Accessor] Failed to update task with ID {Id}", request.Id);
            return Results.Problem("Internal server error while updating task.");
        }
    }

    public static async Task<IResult> DeleteTaskAsync(
        int taskId,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger)
    {
        logger.LogInformation("[Accessor] Received DELETE request for Task ID: {TaskId}", taskId);

        try
        {
            var deleted = await accessorService.DeleteTaskAsync(taskId);
            if (!deleted)
            {
                logger.LogWarning("[Accessor] Task with ID {TaskId} not found.", taskId);
                return Results.NotFound($"Task with ID {taskId} not found.");
            }

            logger.LogInformation("[Accessor] Task with ID {TaskId} deleted successfully.", taskId);
            return Results.Ok($"Task {taskId} deleted.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Accessor] Failed to delete task with ID {TaskId}", taskId);
            return Results.Problem("Internal server error while deleting task.");
        }
    }

    #endregion

}