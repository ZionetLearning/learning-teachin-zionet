using Accessor.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AccessorUnitTests;

public class AccessorEndpointsTestHelpers
{
    public static async Task<IResult> InvokeGetTaskById(
        int id,
        IAccessorService service,
        ILogger<AccessorService> logger)
    {
        try
        {
            var task = await service.GetTaskByIdAsync(id);
            if (task is not null)
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
}