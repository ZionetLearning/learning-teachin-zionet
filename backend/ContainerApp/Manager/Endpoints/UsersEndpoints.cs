using Manager.Models.Users;
using Manager.Services;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/users-manager").WithTags("Users");

        g.MapGet("/user-list", GetAllUsersAsync).WithName("GetAllUsers");
        g.MapGet("/user/{userId:guid}", GetUserAsync).WithName("GetUser");
        g.MapPost("/user", CreateUserAsync).WithName("CreateUser");
        g.MapPut("/user/{userId:guid}", UpdateUserAsync).WithName("UpdateUser");
        g.MapDelete("/user/{userId:guid}", DeleteUserAsync).WithName("DeleteUser");

        return app;
    }

    private static async Task<IResult> GetUserAsync(
        [FromRoute] Guid userId,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using var scope = logger.BeginScope("UserId {UserId}:", userId);
        try
        {
            var user = await managerService.GetUserAsync(userId);
            if (user is null)
            {
                logger.LogWarning("User not found");
                return Results.NotFound($"User with ID {userId} not found.");
            }

            logger.LogInformation("User retrieved");
            return Results.Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user");
            return Results.Problem("Failed to get user.");
        }
    }

    private static async Task<IResult> CreateUserAsync(
        [FromBody] UserModel user,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using var scope = logger.BeginScope("CreateUser {Email}:", user.Email);
        try
        {
            var success = await managerService.CreateUserAsync(user);
            if (!success)
            {
                logger.LogWarning("User creation failed");
                return Results.Conflict("User already exists.");
            }

            logger.LogInformation("User created");
            return Results.Created($"/users-manager/user/{user.UserId}", user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return Results.Problem("Failed to create user.");
        }
    }

    private static async Task<IResult> UpdateUserAsync(
        [FromRoute] Guid userId,
        [FromBody] UpdateUserModel user,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using var scope = logger.BeginScope("UpdateUser {UserId}:", userId);
        try
        {
            var success = await managerService.UpdateUserAsync(user, userId);
            if (!success)
            {
                logger.LogWarning("User not found for update");
                return Results.NotFound("User not found.");
            }

            logger.LogInformation("User updated");
            return Results.Ok("User updated.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user");
            return Results.Problem("Failed to update user.");
        }
    }

    private static async Task<IResult> DeleteUserAsync(
        [FromRoute] Guid userId,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using var scope = logger.BeginScope("DeleteUser {UserId}:", userId);
        try
        {
            var success = await managerService.DeleteUserAsync(userId);
            if (!success)
            {
                logger.LogWarning("User not found for deletion");
                return Results.NotFound("User not found.");
            }

            logger.LogInformation("User deleted");
            return Results.Ok("User deleted.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user");
            return Results.Problem("Failed to delete user.");
        }
    }

    private static async Task<IResult> GetAllUsersAsync(
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<ManagerService> logger)
    {
        using var scope = logger.BeginScope("GetAllUsers:");
        try
        {
            var users = await managerService.GetAllUsersAsync();
            logger.LogInformation("Retrieved {Count} users", users.Count());
            return Results.Ok(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users");
            return Results.Problem("Failed to retrieve users.");
        }
    }
}
