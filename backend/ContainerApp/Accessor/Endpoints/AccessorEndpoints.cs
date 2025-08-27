using System.Text.Json;
using Accessor.Models;
using Accessor.Models.Users;
using Accessor.Services;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class AccessorEndpoints
{
    public static void MapAccessorEndpoints(this WebApplication app)
    {
        #region HTTP GET

        app.MapGet("/task/{id:int}", GetTaskByIdAsync);

        app.MapGet("/chats/{threadId:guid}/{userId:guid}/history", GetHistorySnapshotAsync).WithName("GetHistorySnapshot");

        app.MapGet("/chats/{userId}", GetChatsForUserAsync);

        #endregion

        #region HTTP POST

        app.MapPost("/task", CreateTaskAsync);

        app.MapPost("/chats/history", UpsertHistorySnapshotAsync).WithName("UpsertHistorySnapshot");

        app.MapPost("/auth/login", LoginUserAsync);

        #endregion

        #region HTTP DELETE

        app.MapDelete("/task/{taskId}", DeleteTaskAsync);

        #endregion

        #region Users Endpoints

        app.MapGet("/users/{userId:guid}", GetUserAsync).WithName("GetUser");
        app.MapGet("/users", GetAllUsersAsync).WithName("GetAllUsers");
        app.MapPost("/users", CreateUserAsync).WithName("CreateUser");
        app.MapPut("/users/{userId:guid}", UpdateUserAsync).WithName("UpdateUser");
        app.MapDelete("/users/{userId:guid}", DeleteUserAsync).WithName("DeleteUser");

        #endregion
    }

    #region HandlerMethods
    public static async Task<IResult> GetTaskByIdAsync(
        int id,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger
    )
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
            logger.LogInformation("Task saved successfully.");
            return Results.Ok($"Task {task.Id} Saved");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save task.");
            return Results.Problem("An error occurred while saving the task.");
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
            return Results.Problem("Internal server error while deleting task.");
        }
    }

    #endregion

    #region Chat-History Handlers
    private static async Task<IResult> UpsertHistorySnapshotAsync(
    [FromBody] UpsertHistoryRequest body,
    [FromServices] IAccessorService accessorService,
    [FromServices] ILogger<AccessorService> logger)
    {
        using var _ = logger.BeginScope(
            "Handler: {Handler}, ThreadId: {ThreadId}",
            nameof(UpsertHistorySnapshotAsync), body.ThreadId);

        try
        {
            if (body.ThreadId == Guid.Empty)
            {
                return Results.BadRequest(new { error = "threadId is required and must be a GUID." });

            }

            if (body.UserId == Guid.Empty)
            {
                return Results.BadRequest(new { error = "UserId is required." });

            }

            if (body.History.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                return Results.BadRequest(new { error = "history (raw SK ChatHistory) is required." });

            }

            var existing = await accessorService.GetHistorySnapshotAsync(body.ThreadId);

            var snapshot = new ChatHistorySnapshot
            {
                ThreadId = body.ThreadId,
                UserId = body.UserId,
                ChatType = body.ChatType ?? "default",
                Name = body.Name,
                History = body.History.GetRawText(),
                CreatedAt = existing?.CreatedAt ?? DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await accessorService.UpsertHistorySnapshotAsync(snapshot);

            var historyForResponse = body.History.Clone();

            var payload = new
            {
                threadId = snapshot.ThreadId,
                snapshot.UserId,
                snapshot.ChatType,
                history = historyForResponse
            };

            return existing is null
                ? Results.CreatedAtRoute("GetHistorySnapshot", new { threadId = snapshot.ThreadId }, payload)
                : Results.Ok(payload);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting history snapshot for thread {ThreadId}", body.ThreadId);
            return Results.Problem("An error occurred while storing the chat history snapshot.");
        }
    }

    private static async Task<IResult> GetHistorySnapshotAsync(
      Guid threadId,
      Guid userId,
      [FromServices] IAccessorService accessorService,
      [FromServices] ILogger<AccessorService> logger)
    {
        using var _ = logger.BeginScope(
            "Handler: {Handler}, ThreadId: {ThreadId}",
            nameof(GetHistorySnapshotAsync), threadId);

        try
        {
            var snapshot = await accessorService.GetHistorySnapshotAsync(threadId);
            if (snapshot is null)
            {
                snapshot = new ChatHistorySnapshot
                {
                    ThreadId = threadId,
                    UserId = userId,
                    Name = "New chat",
                    History = """{"messages":[]}""",
                    ChatType = "default",
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                await accessorService.CreateChatAsync(snapshot);
                logger.LogInformation("Created new chat {ChatId}", threadId);

                using var empty = JsonDocument.Parse("""{"messages":[]}""");
                var historyEmpty = empty.RootElement.Clone();
                return Results.Ok(new { threadId, userId = userId, Name = snapshot.Name, chatType = (string?)null, history = historyEmpty });
            }

            if (snapshot.UserId != userId)
            {
                logger.LogError("Error accessing chat history chatID:{ChatId}, userId from request:{UserId}, userId in chat: {UserIdInChat}", threadId, userId, snapshot.UserId);
                // TODO: Return forbidden when userId does not match chat owner
            }

            JsonElement historySafe;
            using (var doc = JsonDocument.Parse(snapshot.History))
            {
                historySafe = doc.RootElement.Clone();
            }

            return Results.Ok(new
            {
                threadId = snapshot.ThreadId,
                snapshot.UserId,
                snapshot.ChatType,
                snapshot.Name,
                history = historySafe
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching history snapshot for thread {ThreadId}", threadId);
            return Results.Problem("An error occurred while retrieving chat history snapshot.");
        }
    }

    private static async Task<IResult> GetChatsForUserAsync(
        Guid userId,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger)
    {
        using var scope = logger.BeginScope("Handler: {Handler}, UserId: {UserId}", nameof(GetChatsForUserAsync), userId);
        try
        {
            var chats = await accessorService.GetChatsForUserAsync(userId);
            logger.LogInformation("Retrieved chats for user");
            return Results.Ok(chats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing chats for user {UserId}", userId);
            return Results.Problem("An error occurred while listing chats.");
        }
    }

    #endregion

    #region Authentication Handlers

    public static async Task<IResult> LoginUserAsync(
    [FromBody] LoginRequest request,
    [FromServices] IAccessorService accessorService,
    [FromServices] ILogger<AccessorService> logger)
    {
        using var scope = logger.BeginScope("Handler: {Handler}, Email: {Email}", nameof(LoginUserAsync), request.Email);
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                logger.LogWarning("Email or password was empty.");
                return Results.BadRequest("Email and password are required.");
            }

            var userId = await accessorService.ValidateCredentialsAsync(request.Email, request.Password);
            if (userId == null)
            {
                logger.LogWarning("Invalid credentials for email: {Email}", request.Email);
                return Results.Unauthorized();
            }

            logger.LogInformation("User logged in successfully.");
            return Results.Ok(userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login failed.");
            return Results.Problem("Internal error during login.");
        }
    }

    #endregion

    #region Users Handlers

    private static async Task<IResult> GetUserAsync(
        [FromRoute] Guid userId,
        [FromServices] IAccessorService service,
        [FromServices] ILogger<IAccessorService> logger)
    {
        using var scope = logger.BeginScope("Method: {Method}, UserId: {UserId}", nameof(GetUserAsync), userId);

        if (userId == Guid.Empty)
        {
            logger.LogWarning("Invalid user ID provided: {UserId}", userId);
            return Results.BadRequest("Invalid user ID.");
        }

        try
        {
            var user = await service.GetUserAsync(userId);
            return user is not null ? Results.Ok(user) : Results.NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve user.");
            return Results.Problem("An error occurred while retrieving the user.");
        }
    }

    private static async Task<IResult> CreateUserAsync(
        [FromBody] UserModel user,
        [FromServices] IAccessorService service,
        [FromServices] ILogger<IAccessorService> logger)
    {
        using var scope = logger.BeginScope("Method: {Method}, UserId: {UserId}", nameof(CreateUserAsync), user.UserId);
        if (user is null)
        {
            logger.LogWarning("User model is null.");
            return Results.BadRequest("User data is required.");
        }

        try
        {
            var created = await service.CreateUserAsync(user);
            return created
                ? Results.Created($"/api/users/{user.UserId}", user)
                : Results.Conflict("User with the same email already exists.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create user.");
            return Results.Problem("An error occurred while creating the user.");
        }
    }

    private static async Task<IResult> UpdateUserAsync(
        [FromRoute] Guid userId,
        [FromBody] UpdateUserModel user,
        [FromServices] IAccessorService service,
        [FromServices] ILogger<IAccessorService> logger)
    {
        using var scope = logger.BeginScope("Method: {Method}, UserId: {UserId}", nameof(UpdateUserAsync), userId);
        if (user is null)
        {
            logger.LogWarning("Update user model is null.");
            return Results.BadRequest("User data is required.");
        }

        if (userId == Guid.Empty)
        {
            logger.LogWarning("Invalid user ID provided: {UserId}", userId);
            return Results.BadRequest("Invalid user ID.");
        }

        try
        {
            var updated = await service.UpdateUserAsync(user, userId);
            return updated ? Results.Ok("User updated") : Results.NotFound("User not found");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update user.");
            return Results.Problem("An error occurred while updating the user.");
        }
    }

    private static async Task<IResult> DeleteUserAsync(
        [FromRoute] Guid userId,
        [FromServices] IAccessorService service,
        [FromServices] ILogger<IAccessorService> logger)
    {
        using var scope = logger.BeginScope("Method: {Method}, UserId: {UserId}", nameof(DeleteUserAsync), userId);
        if (userId == Guid.Empty)
        {
            logger.LogWarning("Invalid user ID provided: {UserId}", userId);
            return Results.BadRequest("Invalid user ID.");
        }

        try
        {
            var deleted = await service.DeleteUserAsync(userId);
            return deleted ? Results.Ok("User deleted") : Results.NotFound("User not found");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete user.");
            return Results.Problem("An error occurred while deleting the user.");
        }
    }

    private static async Task<IResult> GetAllUsersAsync(
        [FromServices] IAccessorService service,
        [FromServices] ILogger<IAccessorService> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("Method: {Method}", nameof(GetAllUsersAsync));

        try
        {
            var users = await service.GetAllUsersAsync();
            logger.LogInformation("Retrieved {Count} users", users.Count());
            return Results.Ok(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve all users.");
            return Results.Problem("An error occurred while retrieving users.");
        }
    }

    #endregion

}