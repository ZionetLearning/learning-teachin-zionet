using System.Security.Claims;
using Azure;
using Azure.Storage.Blobs.Models;
using Manager.Constants;
using Manager.Helpers;
using Manager.Models.Users;
using Manager.Services;
using Manager.Services.Avatars;
using Manager.Services.Avatars.Models;
using Manager.Services.Clients.Accessor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Manager.Endpoints;

public static class UsersEndpoints
{
    private sealed class UserEndpoint { }

    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var usersGroup = app.MapGroup("/users-manager").WithTags("Users");

        usersGroup.MapGet("/user-list", GetAllUsersAsync).WithName("GetAllUsers").RequireAuthorization(PolicyNames.AdminOrTeacher);
        usersGroup.MapGet("/user/{userId:guid}", GetUserAsync).WithName("GetUser").RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);
        usersGroup.MapPost("/user", CreateUserAsync).WithName("CreateUser");
        usersGroup.MapPut("/user/{userId:guid}", UpdateUserAsync).WithName("UpdateUser").RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);
        usersGroup.MapDelete("/user/{userId:guid}", DeleteUserAsync).WithName("DeleteUser").RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);
        usersGroup.MapPut("user/interests/{userId:guid}", SetUserInterestsAsync).WithName("SetUserInterests").RequireAuthorization(PolicyNames.AdminOrStudent);

        usersGroup.MapGet("/teacher/{teacherId:guid}/students", ListStudentsForTeacherAsync).WithName("ListStudentsForTeacher").RequireAuthorization(PolicyNames.AdminOrTeacher);
        usersGroup.MapPost("/teacher/{teacherId:guid}/students/{studentId:guid}", AssignStudentAsync).WithName("AssignStudentToTeacher").RequireAuthorization(PolicyNames.AdminOrTeacher);
        usersGroup.MapDelete("/teacher/{teacherId:guid}/students/{studentId:guid}", UnassignStudentAsync).WithName("UnassignStudentFromTeacher").RequireAuthorization(PolicyNames.AdminOrTeacher);
        usersGroup.MapGet("/student/{studentId:guid}/teachers", ListTeachersForStudentAsync).WithName("ListTeachersForStudent").RequireAuthorization(PolicyNames.AdminOnly);

        usersGroup.MapGet("/online", GetOnlineUsers).WithName("GetOnlineUsers").RequireAuthorization(PolicyNames.AdminOnly);

        usersGroup.MapPost("/user/{userId:guid}/avatar/upload-url", GenerateUploadAvatarUrlAsync).WithName("GetUploadAvatarUrl").RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);
        usersGroup.MapPost("/user/{userId:guid}/avatar/confirm", ConfirmAvatarAsync).WithName("ConfirmAvatar").RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);
        usersGroup.MapDelete("/user/{userId:guid}/avatar", DeleteAvatarAsync).WithName("DeleteAvatar").RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);
        usersGroup.MapGet("/user/{userId:guid}/avatar/url", GenerateAvatarReadUrlAsync).WithName("GetAvatarReadUrl").RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);
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
                HebrewLevelValue = hebrewLevel,
            };

            // Send to accessor
            var success = await accessorClient.CreateUserAsync(user);
            if (!success)
            {
                logger.LogWarning("User creation failed: {Email}", user.Email);
                return Results.Conflict("User could not be created (may already exist or invalid data).");
            }

            // DTO for response 
            var result = new UserCreationResultDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = parsedRole,
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
        [FromServices] ILogger<UserEndpoint> logger,
        HttpContext httpContext)
    {
        using var scope = logger.BeginScope("UpdateUser {UserId}:", userId);

        try
        {
            if (httpContext?.User == null)
            {
                logger.LogWarning("Access denied: missing or unauthenticated user context.");
                return Results.Unauthorized();
            }

            var callerRole = httpContext.User.FindFirst(AuthSettings.RoleClaimType)?.Value;
            if (string.IsNullOrWhiteSpace(callerRole))
            {
                logger.LogWarning("Access denied: no role claim found in user context.");
                return Results.Forbid();
            }

            if (!Enum.TryParse<Role>(callerRole, ignoreCase: true, out var parsedCallerRole))
            {
                logger.LogWarning("Could not determine caller role.");
                return Results.Forbid();
            }

            logger.LogInformation("Caller authenticated with role: {CallerRole}", callerRole);

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

            // only students can have interests
            if (user.Interests is not null && existingUser.Role != Role.Student)
            {
                logger.LogWarning("Non-student tried to set interests. Role: {Role}", existingUser.Role);
                return Results.BadRequest("Only students can set interests.");
            }

            // Only Admins can change role of another user
            if (user.Role.HasValue && parsedCallerRole != Role.Admin)
            {
                logger.LogWarning("Non-admin attempted to change role. Caller role: {CallerRole}", parsedCallerRole);
                return Results.Forbid();
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

        if (string.IsNullOrWhiteSpace(callerRole))
        {
            logger.LogWarning("Unauthorized: missing role.");
            return Results.Unauthorized();
        }

        if (!Guid.TryParse(callerIdRaw, out var callerId))
        {
            logger.LogWarning("Unauthorized: missing or invalid callerId.");
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

        if (string.IsNullOrWhiteSpace(callerRole))
        {
            logger.LogWarning("Unauthorized: missing role.");
            return Results.Unauthorized();
        }

        if (!Guid.TryParse(callerIdRaw, out var callerId))
        {
            logger.LogWarning("Unauthorized: missing or invalid callerId.");
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

        if (string.IsNullOrWhiteSpace(callerRole))
        {
            logger.LogWarning("Unauthorized: missing or empty caller role.");
            return Results.Unauthorized();
        }

        if (!Guid.TryParse(callerIdRaw, out var callerId) || callerId == Guid.Empty)
        {
            logger.LogWarning("Unauthorized: missing or invalid caller id.");
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

    private static async Task<IResult> GetOnlineUsers(
        [FromServices] IOnlinePresenceService onlinePresenceService,
        [FromServices] ILogger<UserEndpoint> logger,
        CancellationToken ct = default)
    {
        try
        {
            var all = await onlinePresenceService.GetOnlineAsync(ct);
            var nonAdmins = all
            .Where(u => !string.Equals(u.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            .ToList();

            return Results.Ok(nonAdmins);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list online users");
            return Results.Problem("Failed to retrieve omline users.");
        }
    }
    private static async Task<IResult> SetUserInterestsAsync(
        [FromRoute] Guid userId,
        [FromBody] UpdateInterestsRequest request,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<UserEndpoint> logger,
        HttpContext httpContext)
    {
        using var scope = logger.BeginScope("SetUserInterests {UserId}:", userId);

        try
        {
            var callerIdRaw = httpContext.User.FindFirstValue(AuthSettings.UserIdClaimType);
            var callerRole = httpContext.User.FindFirstValue(AuthSettings.RoleClaimType);

            if (string.IsNullOrWhiteSpace(callerRole) || !Guid.TryParse(callerIdRaw, out var callerId))
            {
                logger.LogWarning("Unauthorized: missing role or caller ID.");
                return Results.Unauthorized();
            }

            // Fetch target user
            var targetUser = await accessorClient.GetUserAsync(userId);
            if (targetUser is null)
            {
                logger.LogWarning("User {UserId} not found", userId);
                return Results.NotFound("User not found.");
            }

            // Only students can have interests
            if (targetUser.Role != Role.Student)
            {
                logger.LogWarning("Interests can only be set for students. Role: {Role}", targetUser.Role);
                return Results.BadRequest("Only students can have interests.");
            }

            // Authorization: Admins or the student themself
            if (callerRole != Role.Admin.ToString() && callerId != userId)
            {
                logger.LogWarning("Forbidden: caller {CallerId} with role {Role} tried to update interests for {TargetUserId}.", callerId, callerRole, userId);
                return Results.Forbid();
            }

            // Update interests and save
            targetUser.Interests = request.Interests;

            var updateUser = new UpdateUserModel
            {
                Interests = targetUser.Interests
            };

            var updated = await accessorClient.UpdateUserAsync(updateUser, userId);
            return updated ? Results.Ok("Interests updated.") : Results.Problem("Failed to update interests.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set user interests.");
            return Results.Problem("Unexpected error.");
        }
    }

    private static async Task<IResult> GenerateUploadAvatarUrlAsync(
     [FromRoute] Guid userId,
     [FromBody] GetUploadUrlRequest req,
     [FromServices] IAvatarStorageService storage,
     [FromServices] IOptions<AvatarsOptions> opt,
     [FromServices] ILogger<UserEndpoint> logger,
     HttpContext http,
     CancellationToken ct)
    {
        using var _ = logger.BeginScope("UploadUrl: userId={UserId}", userId);

        logger.LogInformation("Request upload-url: ContentType={ContentType}, Size={SizeBytes}", req.ContentType, req.SizeBytes);

        if (!UserDefaultsHelper.IsSelfOrAdmin(http, userId))
        {
            logger.LogWarning("Forbidden for upload-url");
            return Results.Forbid();
        }

        try
        {
            var (url, exp, blobPath) = await storage.GetUploadSasAsync(userId, req.ContentType, req.SizeBytes, ct);

            logger.LogInformation("Generated upload SAS: blobPath={BlobPath}, expiresAt={Expires}", blobPath, exp);

            return Results.Ok(new AvatarUploadUrlResponse
            {
                UploadUrl = url.ToString(),
                BlobPath = blobPath,
                ExpiresAtUtc = exp.UtcDateTime,
                MaxBytes = opt.Value.MaxBytes,
                AcceptedContentTypes = opt.Value.AllowedContentTypes
            });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Validation failed for upload-url");
            return Results.BadRequest(ex.Message);
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Azure request failed");
            return Results.Problem("Storage error.", statusCode: 502);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error");
            return Results.Problem("Unexpected error.");
        }
    }

    private static async Task<IResult> ConfirmAvatarAsync(
        [FromRoute] Guid userId,
        [FromBody] ConfirmAvatarRequest req,
        [FromServices] IAvatarStorageService storage,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] IOptions<AvatarsOptions> opt,
        [FromServices] ILogger<UserEndpoint> log,
        HttpContext http,
        CancellationToken ct)
    {
        using var _ = log.BeginScope("ConfirmAvatar: userId={UserId}", userId);

        if (!UserDefaultsHelper.IsSelfOrAdmin(http, userId))
        {
            log.LogWarning("Forbidden for confirm");
            return Results.Forbid();
        }

        log.LogInformation("Confirm request: BlobPath={BlobPath}, ContentType={CT}", req.BlobPath, req.ContentType);

        var expectedPrefix = $"{userId}/";
        if (!req.BlobPath.StartsWith(expectedPrefix, StringComparison.Ordinal))
        {
            log.LogWarning("Invalid blobPath prefix: expected prefix={Prefix}", expectedPrefix);
            return Results.BadRequest("Invalid blobPath prefix");
        }

        BlobProperties? props;
        try
        {
            props = await storage.GetBlobPropsAsync(req.BlobPath, ct);
        }
        catch (InvalidOperationException ex)
        {
            log.LogWarning(ex, "Validation failed when reading blob props");
            return Results.BadRequest(ex.Message);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            log.LogWarning(ex, "Blob not found during confirm");
            return Results.BadRequest("Blob not found");
        }
        catch (RequestFailedException ex)
        {
            log.LogError(ex, "Azure request failed when reading blob props");
            return Results.Problem("Storage error.", statusCode: 502);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Unexpected error when reading blob props");
            return Results.Problem("Unexpected error.");
        }

        if (props is null)
        {
            log.LogWarning("Blob not found: {BlobPath}", req.BlobPath);
            return Results.BadRequest("Blob not found");
        }

        log.LogInformation("Blob found: Size={Size}, ContentType={BlobCT}, ETag={ETag}",
        props.ContentLength, props.ContentType, props.ETag);

        if (!opt.Value.AllowedContentTypes.Contains(req.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return Results.BadRequest("Unsupported content-type");
        }

        if (req.SizeBytes is > 0 && props.ContentLength != req.SizeBytes)
        {
            log.LogWarning("Client sizeBytes {Client} != blob size {Blob}", req.SizeBytes, props.ContentLength);
        }

        if (props.ContentLength > opt.Value.MaxBytes)
        {
            return Results.BadRequest("File too large");
        }

        if (!string.Equals(props.ContentType, req.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            log.LogWarning("Uploaded content-type {BlobCT} differs from requested {ReqCT}", props.ContentType, req.ContentType);
        }

        if (req.ETag is not null && props.ETag.ToString() != req.ETag)
        {
            log.LogWarning("ETag mismatch: {Blob} != {Req}", props.ETag, req.ETag);
        }

        var user = await accessorClient.GetUserAsync(userId);
        var old = user?.AvatarPath;

        log.LogInformation("Updating avatar in DB. OldPath={Old}, NewPath={New}", old, req.BlobPath);

        var updated = await accessorClient.UpdateUserAsync(new UpdateUserModel
        {
            AvatarPath = req.BlobPath,
            AvatarContentType = req.ContentType
        }, userId);
        if (!updated)
        {
            log.LogError("DB update failed while confirming avatar");
            return Results.NotFound("User not found.");
        }

        if (!string.IsNullOrEmpty(old) && !string.Equals(old, req.BlobPath, StringComparison.Ordinal))
        {
            try
            {
                log.LogInformation("Deleting old avatar: {Old}", old);
                await storage.DeleteAsync(old!, ct);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                log.LogWarning(ex, "Old avatar not found during delete (ignored)");
            }
            catch (RequestFailedException ex)
            {
                log.LogWarning(ex, "Azure delete failed for old avatar (ignored)");
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Unexpected error deleting old avatar (ignored)");
            }
        }

        log.LogInformation("Avatar confirmed successfully");
        return Results.Ok();
    }

    private static async Task<IResult> DeleteAvatarAsync(
        [FromRoute] Guid userId,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] IAvatarStorageService storage,
        [FromServices] ILogger<UserEndpoint> log,
        HttpContext http,
        CancellationToken ct)
    {
        using var _ = log.BeginScope("DeleteAvatar: userId={UserId}", userId);

        if (!UserDefaultsHelper.IsSelfOrAdmin(http, userId))
        {
            log.LogWarning("Forbidden delete avatar");
            return Results.Forbid();
        }

        var user = await accessorClient.GetUserAsync(userId);
        if (user is null)
        {
            log.LogWarning("User not found");
            return Results.NotFound("User not found.");
        }

        if (string.IsNullOrEmpty(user.AvatarPath))
        {
            log.LogInformation("Avatar already empty");
            return Results.Ok();
        }

        try
        {
            await storage.DeleteAsync(user.AvatarPath!, ct);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            log.LogWarning(ex, "Avatar blob not found during delete (ignored)");
        }
        catch (RequestFailedException ex)
        {
            log.LogWarning(ex, "Azure delete failed (ignored)");
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Delete blob failed for {Path}", user.AvatarPath);
        }

        var ok = await accessorClient.UpdateUserAsync(new UpdateUserModel
        {
            AvatarPath = null,
            AvatarContentType = null,
            ClearAvatar = true
        }, userId);

        log.LogInformation("Avatar removed in DB");
        return ok ? Results.Ok() : Results.NotFound("User not found.");
    }

    private static async Task<IResult> GenerateAvatarReadUrlAsync(
        [FromRoute] Guid userId,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] IAvatarStorageService storage,
        [FromServices] IOptions<AvatarsOptions> opt,
        [FromServices] ILogger<UserEndpoint> log,
        HttpContext http,
        CancellationToken ct)
    {
        using var _ = log.BeginScope("ReadUrl: userId={UserId}", userId);

        if (!UserDefaultsHelper.IsSelfOrAdmin(http, userId))
        {
            log.LogWarning("Forbidden for read-url");
            return Results.Forbid();
        }

        var user = await accessorClient.GetUserAsync(userId);
        if (user is null || string.IsNullOrEmpty(user.AvatarPath))
        {
            log.LogWarning("Avatar not set for {UserId}", userId);
            return Results.NotFound();
        }

        try
        {
            var uri = await storage.GenerateReadUrlAsync(user.AvatarPath!, TimeSpan.FromMinutes(opt.Value.ReadUrlTtlMinutes), ct);
            log.LogInformation("Returning read SAS: {Url}", uri);

            return Results.Ok(uri.ToString());
        }
        catch (InvalidOperationException ex)
        {
            log.LogWarning(ex, "Validation failed when generating read SAS");
            return Results.BadRequest(ex.Message);
        }
        catch (RequestFailedException ex)
        {
            log.LogError(ex, "Azure request failed when generating read SAS");
            return Results.Problem("Storage error.", statusCode: 502);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Unexpected error when generating read SAS");
            return Results.Problem("Unexpected error.");
        }
    }
}
