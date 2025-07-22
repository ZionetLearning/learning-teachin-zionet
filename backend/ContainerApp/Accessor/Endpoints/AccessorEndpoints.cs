using Accessor.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using Accessor.Models;

namespace Accessor.Endpoints
{
    public static class AccessorEndpoints
    {
        public static void MapAccessorEndpoints(this WebApplication app)
        {


            #region HTTP POST

            app.MapPost("/emailqueue-input", async (
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

            #endregion


            #region HTTP DELETE

            app.MapDelete("/user/{taskId}", async (int taskId, 
                IAccessorService accessorService, 
                ILogger<Program> logger) =>
            {
                logger.LogInformation($"[Accessor] Received DELETE request for Task ID: {taskId}");

                var deleted = await accessorService.DeleteTaskAsync(taskId);
                if (!deleted)
                {
                    logger.LogWarning($"[Accessor] Task with ID {taskId} not found.");
                    return Results.NotFound($"Task with ID {taskId} not found.");
                }

                logger.LogInformation($"[Accessor] Task with ID {taskId} deleted successfully.");
                return Results.Ok($"Task {taskId} deleted.");
            });



            #endregion







        }
    }
}
