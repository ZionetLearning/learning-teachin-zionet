﻿using Manager.Models.Users;
using Manager.Services.Clients.Accessor;
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
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<UserEndpoint> logger)
    {
        using var scope = logger.BeginScope("UserId {UserId}:", userId);

        try
        {
            var user = await accessorClient.GetUserAsync(userId);
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
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<UserEndpoint> logger)
    {
        using var scope = logger.BeginScope("CreateUser:");

        try
        {
            // Validate role
            if (!Enum.TryParse<Role>(newUser.Role, true, out var parsedRole))
            {
                logger.LogWarning("User creation failed due to invalid role: {RoleInput}", newUser.Role);
                return Results.BadRequest("Invalid role provided.");
            }

            // Build the user model (hash password here!)
            var user = new UserModel
            {
                UserId = newUser.UserId,
                Email = newUser.Email,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password),
                Role = parsedRole
            };

            // Send to accessor
            var success = await accessorClient.CreateUserAsync(user);
            if (!success)
            {
                logger.LogWarning("User creation failed: {Email}", user.Email);
                return Results.Conflict("User could not be created (may already exist or invalid data).");
            }

            // DTO for response (never return raw password)
            var result = new UserData
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = parsedRole
            };

            logger.LogInformation("User {Email} created successfully", user.Email);
            return Results.Created($"/users-manager/user/{user.UserId}", result);
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
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<UserEndpoint> logger)
    {
        using var scope = logger.BeginScope("UpdateUser {UserId}:", userId);

        try
        {
            var success = await accessorClient.UpdateUserAsync(user, userId);
            return success ? Results.Ok("User updated.") : Results.NotFound("User not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user");
            return Results.Problem("Failed to update user.");
        }
    }

    private static async Task<IResult> DeleteUserAsync(
        [FromRoute] Guid userId,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<UserEndpoint> logger)
    {
        using var scope = logger.BeginScope("DeleteUser {UserId}:", userId);

        try
        {
            var success = await accessorClient.DeleteUserAsync(userId);
            return success ? Results.Ok("User deleted.") : Results.NotFound("User not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user");
            return Results.Problem("Failed to delete user.");
        }
    }

    private static async Task<IResult> GetAllUsersAsync(
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<UserEndpoint> logger)
    {
        using var scope = logger.BeginScope("GetAllUsers:");

        try
        {
            var users = await accessorClient.GetAllUsersAsync();
            if (users is null || !users.Any())
            {
                logger.LogWarning("No users found");
                return Results.NotFound("No users found.");
            }

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
