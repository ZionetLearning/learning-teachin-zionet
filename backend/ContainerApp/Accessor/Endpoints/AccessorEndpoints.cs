using Accessor.Constants;
using Accessor.Models;
using Accessor.Services;
using Microsoft.AspNetCore.Mvc;


namespace Accessor.Endpoints
{
    public static class AccessorEndpoints
    {
        public static void MapAccessorEndpoints(this WebApplication app)
        {
            app.MapGet("/task/{id:int}", GetTaskById);
            

            app.MapPost($"/{QueueNames.EngineToAccessor}-input", 
                async (TaskModel task, IAccessorService accessorService, ILogger<AccessorService> logger) =>
            {
                try
                {
                    await accessorService.SaveTaskAsync(task);
                    logger.LogInformation("Task {Id} saved successfully", task.Id);
                    return Results.Ok(new { Status = "Saved", task.Id });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to save task {Id}", task.Id);
                    return Results.Problem("An error occurred while saving the task.");
                }
            });




            app.MapPost($"/{QueueNames.TaskUpdateInput}", async (
            [FromBody] UpdateTaskName request,
            IAccessorService accessorService,
            ILogger<Program> logger) =>
            {
                logger.LogInformation($"[Accessor] Received request to update task. Id: {request.Id}, NewName: {request.Name}");

                try
                {
                    var success = await accessorService.UpdateTaskNameAsync(request.Id, request.Name);
                    if (!success)
                    {
                        logger.LogWarning($"[Accessor] Task with ID {request.Id} not found.");
                        return Results.NotFound($"Task with ID {request.Id} not found.");
                    }

                    logger.LogInformation($"[Accessor] Task {request.Id} updated successfully.");
                    return Results.Ok($"Task {request.Id} updated successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"[Accessor] Failed to update task with ID {request.Id}");
                    return Results.Problem("Internal server error while updating task.");
                }
            });



            app.MapDelete("/task/{taskId}", async (int taskId,
                IAccessorService accessorService,
                ILogger<Program> logger) =>
            {
                logger.LogInformation($"[Accessor] Received DELETE request for Task ID: {taskId}");
                try
                {
                    var deleted = await accessorService.DeleteTaskAsync(taskId);
                    if (!deleted)
                    {
                        logger.LogWarning($"[Accessor] Task with ID {taskId} not found.");
                        return Results.NotFound($"Task with ID {taskId} not found.");
                    }

                    logger.LogInformation($"[Accessor] Task with ID {taskId} deleted successfully.");
                    return Results.Ok($"Task {taskId} deleted.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"[Accessor] Failed to delete task with ID {taskId}");
                    return Results.Problem("Internal server error while deleting task.");
                }

            });
        }

        #region HandlerMethods

        public static async Task<IResult> GetTaskById(
            int id,
            IAccessorService accessorService,
            ILogger<AccessorService> logger)
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

        #endregion
    }
}
