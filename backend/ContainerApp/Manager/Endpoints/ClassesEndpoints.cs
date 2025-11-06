using System.Security.Claims;
using Manager.Constants;
using Manager.Models.Classes;
using Manager.Services.Clients.Accessor;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class ClassesEndpoints
{
    private sealed class ClassEndpoint { }

    public static IEndpointRouteBuilder MapClassesEndpoints(this IEndpointRouteBuilder app)
    {
        var classesGroup = app.MapGroup("/classes-manager").WithTags("Classes");

        classesGroup.MapGet("/{classId:guid}", GetClassAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        classesGroup.MapGet("", GetAllClassesAsync)
            .RequireAuthorization(PolicyNames.AdminOnly);

        classesGroup.MapGet("/my", GetMyClassesAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        classesGroup.MapPost("", CreateClassAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacher);

        classesGroup.MapPost("/{classId:guid}/members", AddMembersAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacher);

        classesGroup.MapDelete("/{classId:guid}/members", RemoveMembersAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacher);

        classesGroup.MapDelete("/{classId:guid}", DeleteClassAsync)
        .RequireAuthorization(PolicyNames.AdminOrTeacher);

        return app;
    }

    private static async Task<IResult> GetClassAsync(
        [FromRoute] Guid classId,
        [FromServices] IAccessorClient accessorClient,
        ILogger<ClassEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            using var scope = logger.BeginScope("ClassID: {ClassId}:", classId);
            logger.LogInformation("Fetching class info");
            var cls = await accessorClient.GetClassAsync(classId, ct);

            return cls is not null ? Results.Ok(cls) : Results.NotFound("Class not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching class");
            return Results.Problem("Failed to retrieve class. Please try again later.");
        }
    }
    private static async Task<IResult> GetAllClassesAsync(
    [FromServices] IAccessorClient accessorClient,
    ILogger<ClassEndpoint> logger,
    CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Fetching all classes");
            var cls = await accessorClient.GetAllClassesAsync(ct);

            return cls is not null ? Results.Ok(cls) : Results.NotFound("Classes not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching classes");
            return Results.Problem("Failed to retrieve classes. Please try again later.");
        }
    }

    private static async Task<IResult> GetMyClassesAsync(
    [FromServices] IAccessorClient accessorClient,
    HttpContext http,
    ILogger<ClassEndpoint> logger,
    CancellationToken ct)
    {
        try
        {
            var callerId = http.User.FindFirstValue(AuthSettings.UserIdClaimType);
            if (callerId == null)
            {
                return Results.BadRequest("UserId is required.");
            }

            using var scope = logger.BeginScope("UserId: {CallerId}:", callerId);
            logger.LogInformation("Fetching classes info");
            var cls = await accessorClient.GetMyClassesAsync(Guid.Parse(callerId), ct);

            return cls is not null ? Results.Ok(cls) : Results.NotFound("Classes not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching classes for user");
            return Results.Problem("Failed to retrieve classes. Please try again later.");
        }
    }

    private static async Task<IResult> CreateClassAsync(
        [FromBody] CreateClassRequest request,
        [FromServices] IAccessorClient accessorClient,
        HttpContext http,
        ILogger<ClassEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            var callerRole = http.User.FindFirstValue(AuthSettings.RoleClaimType);
            var callerId = http.User.FindFirstValue(AuthSettings.UserIdClaimType);
            logger.LogInformation("CreateClass called by {Role} ({CallerId}) for {Name}", callerRole, callerId, request.Name);
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest("Class name is required.");
            }

            var result = await accessorClient.CreateClassAsync(request, ct);
            return result is not null
                ? Results.Created($"/classes-manager/{result.ClassId}", result)
                : Results.Conflict("Class with same name or code already exists.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating class {Name}", request.Name);
            return Results.Problem("Failed to create class. Please try again later.");
        }
    }

    private static async Task<IResult> AddMembersAsync(
        [FromRoute] Guid classId,
        [FromBody] AddMembersRequest request,
        [FromServices] IAccessorClient accessorClient,
        HttpContext http,
        ILogger<ClassEndpoint> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("ClassID: {ClassId}:", classId);
        try
        {
            var callerRole = http.User.FindFirstValue(AuthSettings.RoleClaimType);
            var callerId = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

            logger.LogInformation("AddMembers called by {Role} ({CallerId}) for", callerRole, callerId);

            if (classId == Guid.Empty || request.UserIds is null || !request.UserIds.Any())
            {
                return Results.BadRequest("Invalid classId or empty user list.");
            }

            var ok = await accessorClient.AddMembersToClassAsync(classId, request, ct);
            return ok
                ? Results.Ok(new { message = "Members added successfully." })
                : Results.Problem("Failed to add members.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding members to class");
            return Results.Problem("Failed to add members. Please try again later.");
        }
    }

    private static async Task<IResult> RemoveMembersAsync(
        [FromRoute] Guid classId,
        [FromBody] RemoveMembersRequest request,
        [FromServices] IAccessorClient accessorClient,
        HttpContext http,
        ILogger<ClassEndpoint> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("ClassID: {ClassId}:", classId);
        try
        {
            var callerRole = http.User.FindFirstValue(AuthSettings.RoleClaimType);
            var callerId = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

            logger.LogInformation("RemoveMembers called by {Role} ({CallerId}) for Class", callerRole, callerId);

            if (classId == Guid.Empty || request.UserIds is null || !request.UserIds.Any())
            {
                return Results.BadRequest("Invalid classId or empty user list.");
            }

            var ok = await accessorClient.RemoveMembersFromClassAsync(classId, request, ct);
            return ok
                ? Results.Ok(new { message = "Members removed successfully." })
                : Results.Problem("Failed to remove members.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing members from class");
            return Results.Problem("Failed to remove members. Please try again later.");
        }
    }
    private static async Task<IResult> DeleteClassAsync(
    [FromRoute] Guid classId,
    [FromServices] IAccessorClient accessorClient,
    ILogger<ClassEndpoint> logger,
    CancellationToken ct)
    {
        using var scope = logger.BeginScope("ClassID: {ClassId}:", classId);

        try
        {
            var deleted = await accessorClient.DeleteClassAsync(classId, ct);

            return deleted
                ? Results.NoContent()
                : Results.NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete class ");
            return Results.Problem("Internal server error while deleting class");
        }
    }
}
