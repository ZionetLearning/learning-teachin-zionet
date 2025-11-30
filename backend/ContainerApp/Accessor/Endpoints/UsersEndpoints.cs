using Accessor.Models.Users;
using Accessor.Services;
using Accessor.Services.Avatars;
using Accessor.Services.Avatars.Models;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Azure;
using Azure.Storage.Blobs.Models;

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
        usersGroup.MapPut("/language", UpdateUserLanguageAsync).WithName("UpdateUserLanguage");

        usersGroup.MapPost("/teacher/{teacherId:guid}/students/{studentId:guid}", AssignAsync).WithName("AssignStudentToTeacher_Accessor");

        usersGroup.MapDelete("/teacher/{teacherId:guid}/students/{studentId:guid}", UnassignAsync).WithName("UnassignStudentFromTeacher_Accessor");

        usersGroup.MapGet("/teacher/{teacherId:guid}/students", ListStudentsAsync).WithName("ListStudentsForTeacher_Accessor");

        usersGroup.MapGet("/student/{studentId:guid}/teachers", ListTeachersAsync).WithName("ListTeachersForStudent_Accessor");

        usersGroup.MapPost("/{userId:guid}/avatar/upload-url", GenerateUploadAvatarUrlAsync)
            .WithName("GenerateUploadAvatarUrl");

        usersGroup.MapPost("/{userId:guid}/avatar/confirm", ConfirmAvatarAsync).WithName("ConfirmAvatar");
        usersGroup.MapDelete("/{userId:guid}/avatar", DeleteAvatarAsync).WithName("DeleteAvatar");
        usersGroup.MapGet("/{userId:guid}/avatar/read-url", GenerateAvatarReadUrlAsync).WithName("GenerateAvatarReadUrl");

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

    private static async Task<IResult> UpdateUserLanguageAsync(
        [FromBody] UserLanguage payload,
        [FromServices] IUserService service,
        [FromServices] ILogger<UsersEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        if (payload is null)
        {
            logger.LogInformation("payload is null.");
            return Results.BadRequest();
        }

        if (payload.UserId == Guid.Empty)
        {
            logger.LogInformation("User id is empty.");
            return Results.BadRequest();
        }

        using var _ = logger.BeginScope("Method={Method}, Language={Language}", nameof(UpdateUserLanguageAsync), payload.Language);

        try
        {
            var response = await service.UpdateUserLanguageAsync(payload.UserId, payload.Language, ct);
            if (!response)
            {
                logger.LogInformation("User not found.");
                return Results.NotFound("User not found.");
            }

            return Results.Ok("Language updated.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update user language.");
            return Results.Problem("An error occurred while updating the user language.");
        }
    }

    private static async Task<IResult> GenerateUploadAvatarUrlAsync(
        [FromRoute] Guid userId,
        [FromBody] GetUploadAvatarUrlRequest req,
        [FromServices] IAvatarStorageService storage,
        [FromServices] IOptions<AvatarsOptions> opt,
        [FromServices] ILogger<UsersEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("UploadUrl: userId={UserId}", userId);

        logger.LogInformation("Request upload-url: ContentType={ContentType}, Size={SizeBytes}", req.ContentType, req.SizeBytes);

        try
        {
            var (url, exp, blobPath) = await storage.GetUploadSasAsync(userId, req.ContentType, req.SizeBytes, ct);

            logger.LogInformation("Generated upload SAS: blobPath={BlobPath}, expiresAt={Expires}", blobPath, exp);

            return Results.Ok(new GetUploadAvatarUrlResponse
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
        [FromServices] IUserService userService,
        [FromServices] IOptions<AvatarsOptions> opt,
        [FromServices] ILogger<UsersEndpointsLoggerMarker> log,
        CancellationToken ct)
    {
        using var _ = log.BeginScope("ConfirmAvatar: userId={UserId}", userId);

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

        var user = await userService.GetUserAsync(userId);
        var old = user?.AvatarPath;

        log.LogInformation("Updating avatar in DB. OldPath={Old}, NewPath={New}", old, req.BlobPath);

        var updated = await userService.UpdateUserAsync(new UpdateUserModel
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
        [FromServices] IUserService userService,
        [FromServices] IAvatarStorageService storage,
        [FromServices] ILogger<UsersEndpointsLoggerMarker> log,
        CancellationToken ct)
    {
        using var _ = log.BeginScope("DeleteAvatar: userId={UserId}", userId);

        var user = await userService.GetUserAsync(userId);
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

        var ok = await userService.UpdateUserAsync(new UpdateUserModel
        {
            ClearAvatar = true,
            AvatarPath = null,
            AvatarContentType = null
        }, userId);

        log.LogInformation("Avatar removed in DB");
        return ok ? Results.Ok() : Results.NotFound("User not found.");
    }

    private static async Task<IResult> GenerateAvatarReadUrlAsync(
        [FromRoute] Guid userId,
        [FromServices] IUserService userService,
        [FromServices] IAvatarStorageService storage,
        [FromServices] IOptions<AvatarsOptions> opt,
        [FromServices] ILogger<UsersEndpointsLoggerMarker> log,
        CancellationToken ct)
    {
        using var _ = log.BeginScope("ReadUrl: userId={UserId}", userId);

        var user = await userService.GetUserAsync(userId);
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