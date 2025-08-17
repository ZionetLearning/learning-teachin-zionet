using Accessor.Models;
using Accessor.Services;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class AccessorEndpoints
{
    public static void MapAccessorEndpoints(this WebApplication app)
    {
        #region HTTP GET

        app.MapGet("/task/{id:int}", GetTaskByIdAsync);

        app.MapGet("/threads/{threadId:guid}/messages", GetChatHistoryAsync).WithName("GetChatHistory");

        app.MapGet("/threads/{userId}", GetThreadsForUserAsync);

        #endregion

        #region HTTP POST

        app.MapPost("/task", CreateTaskAsync);

        app.MapPost("/threads/message", StoreMessageAsync);

        #endregion

        #region HTTP DELETE

        app.MapDelete("/task/{taskId}", DeleteTaskAsync);

        #endregion
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
        [FromBody] TaskModel task,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger,
        CancellationToken ct)
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

    #region Chat-History Handlers

    private static async Task<IResult> StoreMessageAsync(
        [FromBody] ChatMessage msg,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger)
    {
        using var scope = logger.BeginScope("Handler: {Handler}, ThreadId: {ThreadId}", nameof(StoreMessageAsync), msg.ThreadId);
        try
        {
            // Validate incoming payload
            if (string.IsNullOrWhiteSpace(msg.Content) || !Enum.IsDefined(typeof(MessageRole), msg.Role))
            {
                logger.LogWarning("Validation failed for message");
                return Results.BadRequest("Role and valid Content are required.");
            }

            // Generate server-side IDs/timestamps
            msg.Id = Guid.NewGuid();
            msg.Timestamp = DateTimeOffset.UtcNow;

            await accessorService.AddMessageAsync(msg);
            logger.LogInformation("Message stored successfully");

            // Return 201 Created with location header
            return Results.CreatedAtRoute(
                routeName: "GetChatHistory",
                routeValues: new { threadId = msg.ThreadId },
                value: msg
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error storing message in thread {ThreadId}", msg.ThreadId);
            return Results.Problem("An error occurred while storing the message.");
        }
    }

    private static async Task<IResult> GetChatHistoryAsync(
        Guid threadId,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger)
    {
        using var scope = logger.BeginScope("Handler: {Handler}, ThreadId: {ThreadId}", nameof(GetChatHistoryAsync), threadId);
        try
        {
            var thread = await accessorService.GetThreadByIdAsync(threadId);
            if (thread is null)
            {
                // Auto-create thread if missing
                thread = new ChatThread
                {
                    ThreadId = threadId,
                    UserId = string.Empty,
                    ChatType = "default",
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                await accessorService.CreateThreadAsync(thread);
                logger.LogInformation("Created new thread {ThreadId}", threadId);
                return Results.Ok(Array.Empty<ChatMessage>());
            }

            var messages = await accessorService.GetMessagesByThreadAsync(threadId);
            logger.LogInformation("Fetched messages for thread {ThreadId}", threadId);
            return Results.Ok(messages);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching history for thread {ThreadId}", threadId);
            return Results.Problem("An error occurred while retrieving chat history.");
        }
    }

    private static async Task<IResult> GetThreadsForUserAsync(
        string userId,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger)
    {
        using var scope = logger.BeginScope("Handler: {Handler}, UserId: {UserId}", nameof(GetThreadsForUserAsync), userId);
        try
        {
            var threads = await accessorService.GetThreadsForUserAsync(userId);
            logger.LogInformation("Retrieved threads for user");
            return Results.Ok(threads);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing threads for user {UserId}", userId);
            return Results.Problem("An error occurred while listing chat threads.");
        }
    }

    #endregion

}