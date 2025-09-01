using Manager.Models.Users;
using Manager.Services;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class UsersEndpoints
{
    private sealed class UserEndpoint { }
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var usersGroup = app.MapGroup("/users-manager").WithTags("Users");

        usersGroup.MapGet("/user-list", GetAllUsersAsync).WithName("GetAllUsers").RequireAuthorization();
        usersGroup.MapGet("/user/{userId:guid}", GetUserAsync).WithName("GetUser").RequireAuthorization();
        usersGroup.MapPost("/user", CreateUserAsync).WithName("CreateUser");
        usersGroup.MapPut("/user/{userId:guid}", UpdateUserAsync).WithName("UpdateUser").RequireAuthorization();
        usersGroup.MapDelete("/user/{userId:guid}", DeleteUserAsync).WithName("DeleteUser").RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetUserAsync(
        [FromRoute] Guid userId,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<UserEndpoint> logger)
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
        [FromBody] CreateUser newUser,
        [FromServices] IManagerService managerService,
        [FromServices] ILogger<UserEndpoint> logger)
    {
        using var scope = logger.BeginScope("CreateUser:");
        try
        {
            if (!Enum.TryParse<Role>(newUser.Role, true, out var parsedRole))
            {

                return Results.BadRequest("Invalid role provided.");
            }

            var user = new UserModel
            {
                UserId = newUser.UserId,
                Email = newUser.Email,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Password = newUser.Password,
                Role = parsedRole
            };

            await managerService.CreateUserAsync(user);

            var result = new UserData
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = parsedRole,
            };

            logger.LogInformation("User created");
            return Results.Created($"/users-manager/user/{user.UserId}", result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "User creation conflict");
            return Results.Conflict("User with the same email already exists.");
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid user data");
            return Results.BadRequest("Invalid user data provided.");
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
        [FromServices] ILogger<UserEndpoint> logger)
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
        [FromServices] ILogger<UserEndpoint> logger)
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
        [FromServices] ILogger<UserEndpoint> logger)
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
