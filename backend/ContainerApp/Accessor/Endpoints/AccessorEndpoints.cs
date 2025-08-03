using Accessor.Constants;
using Accessor.Models;
using Accessor.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


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
        using (logger.BeginScope("Method: {Method}, TaskId: {TaskId}", nameof(GetTaskByIdAsync), id))
        {
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
    }

    public static async Task<IResult> CreateTaskAsync(
        TaskModel task,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger)
    {
        using (logger.BeginScope("Method: {Method}, TaskId: {TaskId}", nameof(CreateTaskAsync), task.Id))
        {
            try
            {
                await accessorService.CreateTaskAsync(task);
                logger.LogInformation("Task saved successfully.");
                return Results.Ok($"Task {task.Id} Saved");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save task.");
                return Results.Problem("An error occurred while saving the task.");
            }
        }
    }

    public static async Task<IResult> UpdateTaskNameAsync(
        [FromBody] UpdateTaskName request,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger)
    {
        using (logger.BeginScope("Method: {Method}, TaskId: {TaskId}, NewName: {NewName}", nameof(UpdateTaskNameAsync), request.Id, request.Name))
        {
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
    }

    public static async Task<IResult> DeleteTaskAsync(
        int taskId,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger)
    {
        using (logger.BeginScope("Method: {Method}, TaskId: {TaskId}", nameof(DeleteTaskAsync), taskId))
        {
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
                return Results.Problem("Internal server error while deleting task.");
            }
        }
    }

    #endregion

}