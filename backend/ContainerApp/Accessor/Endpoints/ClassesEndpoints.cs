using Accessor.Models.Classes;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class ClassesEndpoints
{
    private sealed class ClassesEndpointsLoggerMarker { }

    public static IEndpointRouteBuilder MapClassesEndpoints(this IEndpointRouteBuilder app)
    {
        var classesGroup = app.MapGroup("/classes-accessor").WithTags("Classes");

        classesGroup.MapGet("/{classId:guid}", GetClassAsync).WithName("GetClass");
        classesGroup.MapGet("/my/{userid:guid}", GetMyClassesAsync).WithName("GetMyClasses");
        classesGroup.MapGet("", GetAllClassesAsync).WithName("GetAllClasses");

        classesGroup.MapPost("", CreateClassAsync).WithName("CreateClass");

        classesGroup.MapPost("/{classId:guid}/members", AddMembersAsync).WithName("AddMembersToClass");
        classesGroup.MapDelete("/{classId:guid}/members", RemoveMembersAsync).WithName("RemoveMembersFromClass");
        classesGroup.MapDelete("/{classId:guid}", DeleteClassAsync).WithName("DeleteClass");

        return app;
    }
    private static async Task<IResult> GetClassAsync(
        [FromRoute] Guid classId,
        [FromServices] IClassService service,
        [FromServices] ILogger<ClassesEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, ClassId={ClassId}", nameof(GetClassAsync), classId);

        if (classId == Guid.Empty)
        {
            return Results.BadRequest("Invalid class ID.");
        }

        try
        {
            var cls = await service.GetClassWithMembersAsync(classId, ct);
            return cls is not null ? Results.Ok(cls) : Results.NotFound("Class not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get class {ClassId}", classId);
            return Results.Problem("An error occurred while retrieving the class.");
        }
    }

    private static async Task<IResult> GetMyClassesAsync(
    [FromRoute] Guid userId,
    [FromServices] IClassService service,
    [FromServices] ILogger<ClassesEndpointsLoggerMarker> logger,
    CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, UserId={UserId}", nameof(GetClassAsync), userId);

        if (userId == Guid.Empty)
        {
            return Results.BadRequest("Invalid user ID.");
        }

        try
        {
            var cls = await service.GetClassesForUserWithMembersAsync(userId, ct);
            return cls is not null ? Results.Ok(cls) : Results.NotFound("Class not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get class for user {UserId}", userId);
            return Results.Problem("An error occurred while retrieving the class.");
        }
    }
    private static async Task<IResult> GetAllClassesAsync(
    [FromServices] IClassService service,
    [FromServices] ILogger<ClassesEndpointsLoggerMarker> logger,
    CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}", nameof(GetClassAsync));

        try
        {
            var cls = await service.GetAllClassesAsync(ct);
            return cls is not null ? Results.Ok(cls) : Results.NotFound("Class not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get classes");
            return Results.Problem("An error occurred while retrieving the class.");
        }
    }

    private static async Task<IResult> CreateClassAsync(
        [FromBody] CreateClassRequest model,
        [FromServices] IClassService service,
        [FromServices] ILogger<ClassesEndpointsLoggerMarker> logger, CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, Name={Name}", nameof(CreateClassAsync), model.Name);

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return Results.BadRequest("Class name is required.");
        }

        try
        {
            var newClass = new Class
            {
                ClassId = Guid.NewGuid(),
                Name = model.Name,
                Code = string.Empty,
                Description = model.Description,
                CreatedAt = DateTime.UtcNow,
            };
            var ok = await service.CreateClassAsync(newClass, ct);
            if (ok is not null)
            {
                return Results.Created($"/classes-accessor/{newClass.ClassId}", newClass);
            }
            else
            {
                return Results.Conflict("Class with same name or code already exists.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create class.");
            return Results.Problem("An error occurred while creating the class.");
        }
    }

    private static async Task<IResult> AddMembersAsync(
        [FromRoute] Guid classId,
        [FromBody] AddMembersRequest request,
        [FromServices] IClassService service,
        [FromServices] ILogger<ClassesEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, ClassId={ClassId}", nameof(AddMembersAsync), classId);

        if (classId == Guid.Empty || request.UserIds is null || !request.UserIds.Any())
        {
            return Results.BadRequest("Invalid classId or empty user list.");
        }

        try
        {
            var ok = await service.AddMembersAsync(classId, request.UserIds, request.AddedBy, ct);
            return ok
                ? Results.Ok(new { message = "Members added." })
                : Results.BadRequest(new { error = "Failed to add members." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add members.");
            return Results.Problem("An error occurred while adding members.");
        }
    }

    private static async Task<IResult> RemoveMembersAsync(
        [FromRoute] Guid classId,
        [FromBody] RemoveMembersRequest request,
        [FromServices] IClassService service,
        [FromServices] ILogger<ClassesEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, ClassId={ClassId}", nameof(RemoveMembersAsync), classId);

        if (classId == Guid.Empty || request.UserIds is null || !request.UserIds.Any())
        {
            return Results.BadRequest("Invalid classId or empty user list.");
        }

        try
        {
            var ok = await service.RemoveMembersAsync(classId, request.UserIds, ct);
            return ok
                ? Results.Ok(new { message = "Members removed." })
                : Results.BadRequest(new { error = "Failed to remove members." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove members from class");
            return Results.Problem("An error occurred while removing members.");
        }
    }
    private static async Task<IResult> DeleteClassAsync(
    [FromRoute] Guid classId,
    [FromServices] IClassService service,
    [FromServices] ILogger<ClassesEndpointsLoggerMarker> logger,
    CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, ClassId={ClassId}", nameof(RemoveMembersAsync), classId);

        try
        {
            var deleted = await service.DeleteClassAsync(classId, ct);

            return deleted
                ? Results.NoContent()
                : Results.NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete class with ID");
            return Results.Problem("Internal server error while deleting class");
        }
    }
}

