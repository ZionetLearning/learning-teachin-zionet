using Accessor.Models.Users;
using Accessor.Services;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var usersGroup = app.MapGroup("/users-accessor").WithTags("Users");

        usersGroup.MapGet("/{userId:guid}", GetUserAsync).WithName("GetUser");
        usersGroup.MapGet("", GetAllUsersAsync).WithName("GetAllUsers");
        usersGroup.MapPost("", CreateUserAsync).WithName("CreateUser");
        usersGroup.MapPut("/{userId:guid}", UpdateUserAsync).WithName("UpdateUser");
        usersGroup.MapDelete("/{userId:guid}", DeleteUserAsync).WithName("DeleteUser");

        return app;
    }

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
                ? Results.Created($"/users-accessor/{user.UserId}", user)
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
}
