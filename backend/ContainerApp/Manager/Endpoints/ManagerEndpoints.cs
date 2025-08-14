//using System.IdentityModel.Tokens.Jwt;
using Manager.Helpers;
using Manager.Models;
using Manager.Models.Auth;
using Manager.Models.ModelValidation;
using Manager.Services;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class ManagerEndpoints
{
    public static WebApplication MapManagerEndpoints(this WebApplication app)
    {

        #region HTTP GET

        app.MapGet("/task/{id}", GetTaskAsync).WithName("GetTask");

        #endregion

        #region HTTP POST

        app.MapPost("/task", CreateTaskAsync).WithName("CreateTask");

        app.MapPost("/tasklong", CreateTaskLongAsync).WithName("CreateTaskLongTest");

        #endregion

        #region HTTP PUT

        app.MapPut("/task/{id}/{name}", UpdateTaskNameAsync).WithName("UpdateTaskName");

        #endregion

        #region HTTP DELETE

        app.MapDelete("/task/{id}", DeleteTaskAsync).WithName("DeleteTask");

        #endregion

        #region Authentication and Authorization Endpoints

        app.MapPost("/api/auth/login", LoginAsync).WithName("Login");

        app.MapPost("/api/auth/refresh-tokens", RefreshTokensAsync).WithName("RefreshTokens");

        app.MapPost("/api/auth/logout", LogoutAsync).WithName("Logout");

        app.MapGet("/api/protected", testAuth)
        .RequireAuthorization();s

        #endregion

        return app;
    }

    private static async Task<IResult> GetTaskAsync(
        [FromRoute] int id,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using (logger.BeginScope("Method {MethodName}:", nameof(GetTaskAsync)))
        {
            try
            {
                var task = await managerService.GetTaskAsync(id);
                if (task is not null)
                {
                    logger.LogInformation("Successfully retrieved task");
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
    }

    private static async Task<IResult> CreateTaskAsync(
        [FromBody] TaskModel task,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using (logger.BeginScope("Method: {Method}", nameof(CreateTaskAsync)))
        {
            if (!ValidationExtensions.TryValidate(task, out var validationErrors))
            {
                logger.LogWarning("Validation failed for {Model}: {Errors}", nameof(TaskModel), validationErrors);
                return Results.BadRequest(new { errors = validationErrors });
            }

            try
            {
                logger.LogInformation("Processing task creation for ID {TaskId}", task.Id);

                var (success, message) = await managerService.ProcessTaskAsync(task);
                if (success)
                {
                    logger.LogInformation("Task {TaskId} processed successfully", task.Id);
                    return Results.Accepted($"/task/{task.Id}", new { status = message, task.Id });
                }

                logger.LogWarning("Processing task {TaskId} failed: {Message}", task.Id, message);
                return Results.Problem("Failed to process the task.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing task {TaskId}", task.Id);
                return Results.Problem("An error occurred while processing the task.");
            }
        }
    }

    private static async Task<IResult> CreateTaskLongAsync(
        [FromBody] TaskModel task,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using (logger.BeginScope("Method: {Method}", nameof(CreateTaskAsync)))
        {
            try
            {
                logger.LogInformation("Long running flow test");
                await managerService.ProcessTaskLongAsync(task);
                return Results.Accepted("Long running task accepted");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing task {TaskId}", task.Id);
                return Results.Problem("An error occurred while processing the task.");
            }
        }
    }

    private static async Task<IResult> UpdateTaskNameAsync(
        [FromRoute] int id,
        [FromRoute] string name,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using (logger.BeginScope("Method: {Method}", nameof(UpdateTaskNameAsync)))
        {
            try
            {
                logger.LogInformation("Attempting to update task name for ID {TaskId}", id);

                var success = await managerService.UpdateTaskName(id, name);

                if (success)
                {
                    logger.LogInformation("Successfully updated task name for ID {TaskId}", id);
                    return Results.Ok("Task name updated");
                }

                logger.LogWarning("Task with ID {TaskId} not found for name update", id);
                return Results.NotFound("Task not found");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating task name for ID {TaskId}", id);
                return Results.Problem("An error occurred while updating the task name.");
            }
        }
    }

    private static async Task<IResult> DeleteTaskAsync(
        [FromRoute] int id,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using (logger.BeginScope("Method: {Method}", nameof(DeleteTaskAsync)))
        {
            try
            {
                logger.LogInformation("Attempting to delete task with ID {TaskId}", id);

                var success = await managerService.DeleteTask(id);

                if (success)
                {
                    logger.LogInformation("Successfully deleted task with ID {TaskId}", id);
                    return Results.Ok("Task deleted");
                }

                logger.LogWarning("Task with ID {TaskId} not found for deletion", id);
                return Results.NotFound(new { Message = $"Task with ID {id} not found" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting task with ID {TaskId}", id);
                return Results.Problem("An error occurred while deleting the task.");
            }
        }
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest loginRequest,
        [FromServices] IAuthService authService,
        [FromServices] ILogger<ManagerService> logger,
        HttpRequest httpRequest,
        HttpResponse response)
    {
        using (logger.BeginScope("Method: {Method}", nameof(LoginAsync)))
        {
            try
            {
                logger.LogInformation("Attempting login for {Email}", loginRequest.Email);

                var (accessToken, refreshToken) = await authService.LoginAsync(loginRequest, httpRequest);

                CookieHelper.SetRefreshTokenCookie(response, refreshToken);

                logger.LogInformation("Login successful for {Email}", loginRequest.Email);
                return Results.Ok(new { accessToken });
            }
            catch (UnauthorizedAccessException)
            {
                logger.LogWarning("Unauthorized login attempt for {Email}", loginRequest.Email);
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during login for {Email}", loginRequest.Email);
                return Results.Problem("An unexpected error occurred during login.");
            }
        }
    }

    private static async Task<IResult> RefreshTokensAsync(
        [FromServices] IAuthService authService,
        [FromServices] ILogger<ManagerService> logger,
        HttpRequest request,
        HttpResponse response)
    {
        using (logger.BeginScope("Method: {Method}", nameof(RefreshTokensAsync)))
        {
            try
            {
                var (accessToken, newRefreshToken) = await authService.RefreshTokensAsync(request);

                CookieHelper.SetRefreshTokenCookie(response, newRefreshToken);

                logger.LogInformation("Refresh token successful");
                return Results.Ok(new { accessToken });
            }
            catch (UnauthorizedAccessException)
            {
                logger.LogWarning("Refresh token request unauthorized");
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error refreshing token");
                return Results.Problem("Failed to refresh token");
            }
        }
    }

    private static async Task<IResult> LogoutAsync(
        [FromServices] IAuthService authService,
        [FromServices] ILogger<ManagerService> logger,
        HttpRequest request,
        HttpResponse response)
    {
        using (logger.BeginScope("Method: {Method}", nameof(LogoutAsync)))
        {
            try
            {
                await authService.LogoutAsync(request);

                // Clear the refresh token cookie
                CookieHelper.ClearRefreshTokenCookie(response);

                logger.LogInformation("Logout successful");
                return Results.Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during logout");
                return Results.Problem("An error occurred during logout.");
            }
        }
    }

    private static Task<IResult> testAuth(
        [FromServices] IAuthService authService,
        [FromServices] ILogger<ManagerService> logger,
        HttpRequest request,
        HttpResponse response)
    {
        using (logger.BeginScope("Method: {Method}", nameof(LogoutAsync)))
        {
            try
            {
                logger.LogInformation("YOu secceude!!");
                return Task.FromResult(Results.Ok(new { message = "You are authenticated!" }));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during logout");
                return Task.FromResult(Results.Problem());
            }
        }
    }
}