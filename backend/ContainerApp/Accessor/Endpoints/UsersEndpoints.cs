using Accessor.Models.Users;
using Accessor.Services;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class UsersEndpoints
{
    private sealed class UsersEndpointsLoggerMarker { }

    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var usersGroup = app.MapGroup("/users-accessor").WithTags("Users");

        usersGroup.MapGet("/{userId:guid}", GetUserAsync).WithName("GetUser");
        usersGroup.MapGet("", GetAllUsersAsync).WithName("GetAllUsers");

        usersGroup.MapGet("/{userId:guid}/interests", GetUserInterestsAsync)
            .WithName("GetUserInterests");

        usersGroup.MapPost("", CreateUserAsync).WithName("CreateUser");
        usersGroup.MapPut("/{userId:guid}", UpdateUserAsync).WithName("UpdateUser");
        usersGroup.MapDelete("/{userId:guid}", DeleteUserAsync).WithName("DeleteUser");

        usersGroup.MapPost("/teacher/{teacherId:guid}/students/{studentId:guid}", AssignAsync).WithName("AssignStudentToTeacher_Accessor");

        usersGroup.MapDelete("/teacher/{teacherId:guid}/students/{studentId:guid}", UnassignAsync).WithName("UnassignStudentFromTeacher_Accessor");

        usersGroup.MapGet("/teacher/{teacherId:guid}/students", ListStudentsAsync).WithName("ListStudentsForTeacher_Accessor");

        usersGroup.MapGet("/student/{studentId:guid}/teachers", ListTeachersAsync).WithName("ListTeachersForStudent_Accessor");

        return app;
    }

    private static async Task<IResult> GetUserAsync(
        [FromRoute] Guid userId,
        [FromServices] IUserService userService,
        [FromServices] ILogger<UserService> logger)
    {
        using var scope = logger.BeginScope("Method: {Method}, UserId: {UserId}", nameof(GetUserAsync), userId);

        if (userId == Guid.Empty)
        {
            logger.LogWarning("Invalid user ID provided: {UserId}", userId);
            return Results.BadRequest("Invalid user ID.");
        }

        try
        {
            var user = await userService.GetUserAsync(userId);
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
        [FromServices] IUserService userService,
        [FromServices] ILogger<UserService> logger)
    {
        using var scope = logger.BeginScope("Method: {Method}, UserId: {UserId}", nameof(CreateUserAsync), user.UserId);
        if (user is null)
        {
            logger.LogWarning("User model is null.");
            return Results.BadRequest("User data is required.");
        }

        try
        {
            var created = await userService.CreateUserAsync(user);
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
        [FromServices] IUserService userService,
        [FromServices] ILogger<UserService> logger)
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
            var updated = await userService.UpdateUserAsync(user, userId);
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
        [FromServices] IUserService userService,
        [FromServices] ILogger<UserService> logger)
    {
        using var scope = logger.BeginScope("Method: {Method}, UserId: {UserId}", nameof(DeleteUserAsync), userId);
        if (userId == Guid.Empty)
        {
            logger.LogWarning("Invalid user ID provided: {UserId}", userId);
            return Results.BadRequest("Invalid user ID.");
        }

        try
        {
            var deleted = await userService.DeleteUserAsync(userId);
            return deleted ? Results.Ok("User deleted") : Results.NotFound("User not found");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete user.");
            return Results.Problem("An error occurred while deleting the user.");
        }
    }

    private static async Task<IResult> GetAllUsersAsync(
        [FromQuery] string? callerRole,
        [FromQuery] Guid? callerId,
        [FromServices] IUserService service,
        [FromServices] ILogger<UserService> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("Method: {Method}", nameof(GetAllUsersAsync));

        try
        {
            // Parse role (if provided)
            Role? roleEnum = null;
            if (!string.IsNullOrWhiteSpace(callerRole) &&
                Enum.TryParse<Role>(callerRole, true, out var parsed))
            {
                roleEnum = parsed;
            }

            // Admin or missing role → return all users
            if (roleEnum is null || roleEnum == Role.Admin)
            {
                var all = await service.GetAllUsersAsync(roleFilter: null, teacherId: null, ct);
                logger.LogInformation("Returned {Count} users (admin/all)", all.Count());
                return Results.Ok(all);
            }

            if (callerId is null || callerId == Guid.Empty)
            {
                logger.LogWarning("Missing callerId for role {Role}", roleEnum);
                return Results.BadRequest("callerId is required.");
            }

            if (roleEnum == Role.Teacher)
            {
                var students = await service.GetStudentsForTeacherAsync(callerId.Value, ct);
                logger.LogInformation("Returned {Count} students for teacher {TeacherId}", students.Count(), callerId);
                return Results.Ok(students);
            }

            // Unknown role (shouldn't happen)
            return Results.BadRequest("Unsupported role.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve users.");
            return Results.Problem("An error occurred while retrieving users.");
        }
    }

    private static async Task<IResult> AssignAsync(
        [FromRoute] Guid teacherId,
        [FromRoute] Guid studentId,
        [FromServices] IUserService service,
        [FromServices] ILogger<UsersEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, TeacherId={TeacherId}, StudentId={StudentId}",
            nameof(AssignAsync), teacherId, studentId);

        if (teacherId == Guid.Empty || studentId == Guid.Empty)
        {
            logger.LogWarning("Invalid teacherId ({TeacherId}) or studentId ({StudentId}).", teacherId, studentId);
            return Results.BadRequest(new { error = "Invalid teacherId or studentId." });
        }

        try
        {
            var ok = await service.AssignStudentToTeacherAsync(teacherId, studentId, ct);
            if (!ok)
            {
                logger.LogWarning("Assign failed.");
                logger.LogInformation("Student {StudentId} assigned to teacher {TeacherId}.", studentId, teacherId);
                return Results.BadRequest(new { error = "Invalid teacher/student or assign failed." });
            }

            return Results.Ok(new { message = "Assigned" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to assign student to teacher.");
            return Results.Problem("An error occurred while assigning the student.");
        }
    }

    private static async Task<IResult> UnassignAsync(
        [FromRoute] Guid teacherId,
        [FromRoute] Guid studentId,
        [FromServices] IUserService service,
        [FromServices] ILogger<UsersEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, TeacherId={TeacherId}, StudentId={StudentId}",
            nameof(UnassignAsync), teacherId, studentId);

        if (teacherId == Guid.Empty || studentId == Guid.Empty)
        {
            logger.LogWarning("Invalid teacherId ({TeacherId}) or studentId ({StudentId}).", teacherId, studentId);
            return Results.BadRequest(new { error = "Invalid teacherId or studentId." });
        }

        try
        {
            var ok = await service.UnassignStudentFromTeacherAsync(teacherId, studentId, ct);
            if (ok)
            {
                logger.LogInformation("Successfully unassigned student {StudentId} from teacher {TeacherId}.",
                    studentId, teacherId);
                return Results.Ok(new { message = "Unassigned" });
            }
            else
            {
                logger.LogWarning("Unassign failed: student {StudentId} was not unassigned from teacher {TeacherId}.",
                    studentId, teacherId);
                return Results.BadRequest(new { error = "Unassign failed." });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to unassign student from teacher.");
            return Results.Problem("An error occurred while unassigning the student.");
        }
    }

    private static async Task<IResult> ListStudentsAsync(
        [FromRoute] Guid teacherId,
        [FromServices] IUserService service,
        [FromServices] ILogger<UsersEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, TeacherId={TeacherId}", nameof(ListStudentsAsync), teacherId);

        if (teacherId == Guid.Empty)
        {
            logger.LogWarning("Invalid teacherId.");
            return Results.BadRequest(new { error = "Invalid teacherId." });
        }

        try
        {
            var list = await service.GetStudentsForTeacherAsync(teacherId, ct);

            logger.LogInformation("Returned {Count} students for teacher {TeacherId}", list?.Count() ?? 0, teacherId);
            return Results.Ok(list ?? Enumerable.Empty<UserData>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list students for teacher.");
            return Results.Problem("An error occurred while retrieving students.");
        }
    }

    private static async Task<IResult> ListTeachersAsync(
        [FromRoute] Guid studentId,
        [FromServices] IUserService service,
        [FromServices] ILogger<UsersEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, StudentId={StudentId}", nameof(ListTeachersAsync), studentId);

        if (studentId == Guid.Empty)
        {
            logger.LogWarning("Invalid studentId.");
            return Results.BadRequest(new { error = "Invalid studentId." });
        }

        try
        {
            var list = await service.GetTeachersForStudentAsync(studentId, ct);

            if (list is null || !list.Any())
            {
                logger.LogInformation("No teachers found for student {StudentId}", studentId);
                return Results.NotFound(new { message = "No teachers found for this student." });
            }

            logger.LogInformation("Returned {Count} teachers for student {StudentId}", list.Count(), studentId);
            return Results.Ok(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list teachers for student.");
            return Results.Problem("An error occurred while retrieving teachers.");
        }
    }

    private static async Task<IResult> GetUserInterestsAsync(
        [FromRoute] Guid userId,
        [FromServices] IUserService service,
        [FromServices] ILogger<UsersEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, UserId={UserId}", nameof(GetUserInterestsAsync), userId);

        if (userId == Guid.Empty)
        {
            logger.LogWarning("Invalid userId.");
            return Results.BadRequest(new { error = "Invalid userId." });
        }

        try
        {
            var interests = await service.GetUserInterestsAsync(userId, ct);

            return interests == null ? Results.NotFound(new { message = "No interests found for user." }) : Results.Ok(interests);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve interests for user {UserId}", userId);
            return Results.Problem("An error occurred while retrieving user interests.");
        }
    }
}