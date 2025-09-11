using System.Security.Claims;
using Manager.Constants;
using Manager.Models.Users;
using Manager.Services.Clients.Accessor;
using Manager.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class UsersEndpoints
{
    private sealed class UserEndpoint { }

    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var usersGroup = app.MapGroup("/users-manager").WithTags("Users");

        usersGroup.MapGet("/user-list", GetAllUsersAsync).WithName("GetAllUsers").RequireAuthorization("AdminOrTeacher");
        usersGroup.MapGet("/user/{userId:guid}", GetUserAsync).WithName("GetUser").RequireAuthorization("AdminOrTeacherOrStudent");
        usersGroup.MapPost("/user", CreateUserAsync).WithName("CreateUser");
        usersGroup.MapPut("/user/{userId:guid}", UpdateUserAsync).WithName("UpdateUser").RequireAuthorization("AdminOrTeacherOrStudent");
        usersGroup.MapDelete("/user/{userId:guid}", DeleteUserAsync).WithName("DeleteUser").RequireAuthorization("AdminOrTeacherOrStudent");

        usersGroup.MapGet("/teacher/{teacherId:guid}/students", ListStudentsForTeacherAsync).WithName("ListStudentsForTeacher").RequireAuthorization("AdminOrTeacher");
        usersGroup.MapPost("/teacher/{teacherId:guid}/students/{studentId:guid}", AssignStudentAsync).WithName("AssignStudentToTeacher").RequireAuthorization("AdminOrTeacher");
        usersGroup.MapDelete("/teacher/{teacherId:guid}/students/{studentId:guid}", UnassignStudentAsync).WithName("UnassignStudentFromTeacher").RequireAuthorization("AdminOrTeacher");
        usersGroup.MapGet("/student/{studentId:guid}/teachers", ListTeachersForStudentAsync).WithName("ListTeachersForStudent").RequireAuthorization("AdminOnly");

        return app;
    }
    private static bool IsTeacher(string? role) =>
    string.Equals(role, Role.Teacher.ToString(), StringComparison.OrdinalIgnoreCase);

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
        [FromServices] ILogger<UserEndpoint> logger,
        HttpContext httpContext)
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

            // Detect UI language from Accept-Language header
            var acceptLanguage = httpContext.Request.Headers["Accept-Language"].FirstOrDefault();
            var sanitizedAcceptLanguage = acceptLanguage?.Replace("\r", string.Empty).Replace("\n", string.Empty);
            logger.LogInformation("Raw Accept-Language header received: {Header}", sanitizedAcceptLanguage ?? "<null>");

            var preferredLanguage = UserDefaultsHelper.ParsePreferredLanguage(acceptLanguage);
            logger.LogInformation("Parsed PreferredLanguageCode: {PreferredLanguage}", preferredLanguage);

            // HebrewLevel only applies to students
            var hebrewLevel = UserDefaultsHelper.GetDefaultHebrewLevel(parsedRole);

            // Build the user model (hash password here!)
            var user = new UserModel
            {
                UserId = newUser.UserId,
                Email = newUser.Email,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password),
                Role = parsedRole,
                PreferredLanguageCode = preferredLanguage,
                HebrewLevelValue = hebrewLevel
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
                Role = parsedRole,
                PreferredLanguageCode = preferredLanguage,
                HebrewLevelValue = hebrewLevel
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
            // Fetch current user to check role
            var existingUser = await accessorClient.GetUserAsync(userId);
            if (existingUser is null)
            {
                logger.LogWarning("User {UserId} not found", userId);
                return Results.NotFound("User not found.");
            }

            if (user.PreferredLanguageCode.HasValue &&
                !Enum.IsDefined(typeof(SupportedLanguage), user.PreferredLanguageCode.Value))
            {
                logger.LogWarning("Invalid PreferredLanguageCode provided: {Language}", user.PreferredLanguageCode);
                return Results.BadRequest("Invalid preferred language.");
            }

            if (existingUser.Role == Role.Student &&
                user.HebrewLevelValue.HasValue &&
                !Enum.IsDefined(typeof(HebrewLevel), user.HebrewLevelValue.Value))
            {
                logger.LogWarning("Invalid HebrewLevelValue provided for student: {HebrewLevel}", user.HebrewLevelValue);
                return Results.BadRequest("Invalid Hebrew level.");
            }

            if (existingUser.Role != Role.Student && user.HebrewLevelValue.HasValue)
            {
                logger.LogWarning("Non-student tried to set HebrewLevel. Role: {Role}", existingUser.Role);
                return Results.BadRequest("Hebrew level can only be set for students.");
            }

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
        [FromServices] ILogger<UserEndpoint> logger,
        HttpContext http,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("GetAllUsers");

        var callerRole = http.User.FindFirstValue(AuthSettings.RoleClaimType);
        var callerIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

        if (string.IsNullOrWhiteSpace(callerRole) || !Guid.TryParse(callerIdRaw, out var callerId))
        {
            logger.LogWarning("Missing/invalid caller context (role/userId)");
            return Results.Unauthorized();
        }

        try
        {
            var users = await accessorClient.GetUsersForCallerAsync(
                new CallerContextDto { CallerRole = callerRole, CallerId = callerId },
                ct);

            if (users is null || !users.Any())
            {
                logger.LogInformation("No users visible to {CallerId} ({Role})", callerId, callerRole);
                return Results.NotFound("No users found.");
            }

            logger.LogInformation("Returned {Count} users for {CallerId} ({Role})",
                users.Count(), callerId, callerRole);
            return Results.Ok(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve users");
            return Results.Problem("Failed to retrieve users.");
        }
    }
    private static async Task<IResult> ListStudentsForTeacherAsync(
        [FromRoute] Guid teacherId,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<UserEndpoint> logger,
        HttpContext http,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, TeacherId={TeacherId}", nameof(ListStudentsForTeacherAsync), teacherId);

        if (teacherId == Guid.Empty)
        {
            logger.LogWarning("Invalid teacherId.");
            return Results.BadRequest("Invalid teacherId.");
        }

        var callerRole = http.User.FindFirstValue(AuthSettings.RoleClaimType);
        var callerIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

        if (!Guid.TryParse(callerIdRaw, out var callerId))
        {
            logger.LogWarning("Unauthorized: missing caller id.");
            return Results.Unauthorized();
        }

        // Teacher can list only their own students; Admin can list any
        if (IsTeacher(callerRole) && callerId != teacherId)
        {
            logger.LogWarning("Forbidden: teacher {CallerId} tried to list students for {TeacherId}.", callerId, teacherId);
            return Results.Forbid();
        }

        try
        {
            var students = await accessorClient.GetStudentsForTeacherAsync(teacherId, ct);
            return Results.Ok(students);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list students for teacher.");
            return Results.Problem("Failed to retrieve students.");
        }
    }

    private static async Task<IResult> AssignStudentAsync(
        [FromRoute] Guid teacherId,
        [FromRoute] Guid studentId,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<UserEndpoint> logger,
        HttpContext http,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, TeacherId={TeacherId}, StudentId={StudentId}",
            nameof(AssignStudentAsync), teacherId, studentId);

        if (teacherId == Guid.Empty || studentId == Guid.Empty)
        {
            logger.LogWarning("Invalid teacherId or studentId.");
            return Results.BadRequest(new { error = "Invalid teacherId or studentId." });
        }

        var callerRole = http.User.FindFirstValue(AuthSettings.RoleClaimType);
        var callerIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

        if (!Guid.TryParse(callerIdRaw, out var callerId))
        {
            logger.LogWarning("Unauthorized: missing caller id.");
            return Results.Unauthorized();
        }

        // Teacher can assign only to themselves; Admin can assign anywhere
        if (IsTeacher(callerRole) && callerId != teacherId)
        {
            logger.LogWarning("Forbidden: teacher {CallerId} tried to assign for {TeacherId}.", callerId, teacherId);
            return Results.Forbid();
        }

        try
        {
            var ok = await accessorClient.AssignStudentToTeacherAsync(
                new TeacherStudentMapDto { TeacherId = teacherId, StudentId = studentId },
                ct);
            return ok ? Results.Ok(new { message = "Assigned" })
                      : Results.BadRequest(new { error = "Assign failed" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to assign student to teacher.");
            return Results.Problem("Failed to assign student.");
        }
    }

    private static async Task<IResult> UnassignStudentAsync(
        [FromRoute] Guid teacherId,
        [FromRoute] Guid studentId,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<UserEndpoint> logger,
        HttpContext http,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, TeacherId={TeacherId}, StudentId={StudentId}",
            nameof(UnassignStudentAsync), teacherId, studentId);

        if (teacherId == Guid.Empty || studentId == Guid.Empty)
        {
            logger.LogWarning("Invalid teacherId or studentId.");
            return Results.BadRequest(new { error = "Invalid teacherId or studentId." });
        }

        var callerRole = http.User.FindFirstValue(AuthSettings.RoleClaimType);
        var callerIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

        if (!Guid.TryParse(callerIdRaw, out var callerId))
        {
            logger.LogWarning("Unauthorized: missing caller id.");
            return Results.Unauthorized();
        }

        if (IsTeacher(callerRole) && callerId != teacherId)
        {
            logger.LogWarning("Forbidden: teacher {CallerId} tried to unassign for {TeacherId}.", callerId, teacherId);
            return Results.Forbid();
        }

        try
        {
            var ok = await accessorClient.UnassignStudentFromTeacherAsync(
                new TeacherStudentMapDto { TeacherId = teacherId, StudentId = studentId },
                ct);
            return ok ? Results.Ok(new { message = "Unassigned" })
                      : Results.BadRequest(new { error = "Unassign failed" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to unassign student from teacher.");
            return Results.Problem("Failed to unassign student.");
        }
    }

    private static async Task<IResult> ListTeachersForStudentAsync(
        [FromRoute] Guid studentId,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<UserEndpoint> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, StudentId={StudentId}", nameof(ListTeachersForStudentAsync), studentId);

        if (studentId == Guid.Empty)
        {
            logger.LogWarning("Invalid studentId.");
            return Results.BadRequest("Invalid studentId.");
        }

        try
        {
            var teachers = await accessorClient.GetTeachersForStudentAsync(studentId, ct);
            return Results.Ok(teachers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list teachers for student.");
            return Results.Problem("Failed to retrieve teachers.");
        }
    }
}
