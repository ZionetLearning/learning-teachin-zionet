using Accessor.Exceptions;
using Accessor.Mapping;
using Accessor.Models.Classes.Requests;
using Accessor.Models.Classes.Responses;
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
            var dbModel = await service.GetClassWithMembersAsync(classId, ct);
            if (dbModel is null)
            {
                return Results.NotFound("Class not found.");
            }

            var response = dbModel.ToResponse();
            return Results.Ok(response);
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
        using var _ = logger.BeginScope("Method={Method}, UserId={UserId}", nameof(GetMyClassesAsync), userId);

        if (userId == Guid.Empty)
        {
            return Results.BadRequest("Invalid user ID.");
        }

        try
        {
            var dbModels = await service.GetClassesForUserWithMembersAsync(userId, ct);
            if (dbModels is null || dbModels.Count == 0)
            {
                return Results.NotFound("Classes not found.");
            }

            var response = dbModels.ToMyClassesResponse();
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get classes for user {UserId}", userId);
            return Results.Problem("An error occurred while retrieving the classes.");
        }
    }
    private static async Task<IResult> GetAllClassesAsync(
        [FromServices] IClassService service,
        [FromServices] ILogger<ClassesEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}", nameof(GetAllClassesAsync));

        try
        {
            var dbModels = await service.GetAllClassesAsync(ct);
            if (dbModels is null || dbModels.Count == 0)
            {
                return Results.NotFound("Classes not found.");
            }

            var response = dbModels.ToResponse();
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get classes");
            return Results.Problem("An error occurred while retrieving the classes.");
        }
    }

    private static async Task<IResult> CreateClassAsync(
        [FromBody] CreateClassRequest request,
        [FromServices] IClassService service,
        [FromServices] ILogger<ClassesEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, Name={Name}", nameof(CreateClassAsync), request.Name);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest("Class name is required.");
        }

        try
        {
            var dbModel = request.ToDbModel();
            var createdClass = await service.CreateClassAsync(dbModel, ct);
            var response = createdClass.ToResponse();
            return Results.Created($"/classes-accessor/{response.ClassId}", response);
        }
        catch (ConflictException ex)
        {
            logger.LogWarning(ex, "Failed to create class due to conflict: {Message}", ex.Message);
            return Results.Conflict("Class with same name or code already exists.");
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

        if (classId == Guid.Empty || request.UserIds is null || request.UserIds.Count == 0)
        {
            return Results.BadRequest("Invalid classId or empty user list.");
        }

        try
        {
            var success = await service.AddMembersAsync(classId, request.UserIds.ToList(), request.AddedBy, ct);
            var response = new AddMembersResponse { Success = success };
            return success
                ? Results.Ok(response)
                : Results.BadRequest(response);
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

        if (classId == Guid.Empty || request.UserIds is null || request.UserIds.Count == 0)
        {
            return Results.BadRequest("Invalid classId or empty user list.");
        }

        try
        {
            var success = await service.RemoveMembersAsync(classId, request.UserIds.ToList(), ct);
            var response = new RemoveMembersResponse { Success = success };
            return success
                ? Results.Ok(response)
                : Results.BadRequest(response);
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
        using var _ = logger.BeginScope("Method={Method}, ClassId={ClassId}", nameof(DeleteClassAsync), classId);

        if (classId == Guid.Empty)
        {
            return Results.BadRequest("Invalid class ID.");
        }

        try
        {
            var success = await service.DeleteClassAsync(classId, ct);
            var response = new DeleteClassResponse { Success = success };

            return success
                ? Results.NoContent()
                : Results.NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete class with ID {ClassId}", classId);
            return Results.Problem("Internal server error while deleting class");
        }
    }
}

