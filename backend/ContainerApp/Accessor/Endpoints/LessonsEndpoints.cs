using Accessor.Mapping;
using Accessor.Models.Lessons.Requests;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Endpoints;

public static class LessonsEndpoints
{
    private sealed class LessonsEndpointsLoggerMarker { }

    public static IEndpointRouteBuilder MapLessonsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/lessons-accessor").WithTags("Lessons");

        group.MapGet("/user/{userId:guid}", GetLessonsByTeacherAsync)
            .WithName("GetLessonsByTeacher");
        group.MapPost("", CreateLessonAsync)
            .WithName("CreateLesson");
        group.MapPut("/{lessonId:guid}", UpdateLessonAsync)
            .WithName("UpdateLesson");
        group.MapDelete("/{lessonId:guid}", DeleteLessonAsync)
            .WithName("DeleteLesson");

        return app;
    }

    private static async Task<IResult> GetLessonsByTeacherAsync(
        [FromRoute] Guid userId,
        [FromServices] ILessonService lessonService,
        [FromServices] ILogger<LessonsEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, UserId={UserId}", nameof(GetLessonsByTeacherAsync), userId);

        if (userId == Guid.Empty)
        {
            return Results.BadRequest("UserId cannot be empty.");
        }

        try
        {
            var dbModels = await lessonService.GetLessonsByTeacherAsync(userId, ct);
            var response = dbModels.ToResponseList();
            logger.LogInformation("Retrieved {Count} lessons for teacher {UserId}", response.Count, userId);
            return Results.Ok(response);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while retrieving lessons for teacher {UserId}", userId);
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving lessons for teacher {UserId}", userId);
            return Results.Problem("An error occurred while fetching lessons.");
        }
    }

    private static async Task<IResult> CreateLessonAsync(
        [FromBody] CreateLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ILogger<LessonsEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, TeacherId={TeacherId}", nameof(CreateLessonAsync), request?.TeacherId);

        if (request == null)
        {
            return Results.BadRequest("Request body cannot be null.");
        }

        if (request.TeacherId == Guid.Empty)
        {
            return Results.BadRequest("TeacherId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Results.BadRequest("Title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return Results.BadRequest("Description cannot be empty.");
        }

        if (request.ContentSections == null || request.ContentSections.Count == 0)
        {
            return Results.BadRequest("ContentSections cannot be empty.");
        }

        try
        {
            var dbModel = await lessonService.CreateLessonAsync(request, ct);
            var response = dbModel.ToResponse();
            logger.LogInformation("Created lesson {LessonId} for teacher {TeacherId}", response.LessonId, request.TeacherId);
            return Results.Created($"/lessons-accessor/{response.LessonId}", response);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while creating lesson for teacher {TeacherId}", request.TeacherId);
            return Results.StatusCode(499);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating lesson for teacher {TeacherId}. Teacher may not exist.", request.TeacherId);
            return Results.Problem("Database error occurred. Teacher may not exist.", statusCode: 422);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating lesson for teacher {TeacherId}", request.TeacherId);
            return Results.Problem("An error occurred while creating the lesson.");
        }
    }

    private static async Task<IResult> UpdateLessonAsync(
        [FromRoute] Guid lessonId,
        [FromBody] UpdateLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ILogger<LessonsEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, LessonId={LessonId}", nameof(UpdateLessonAsync), lessonId);

        if (lessonId == Guid.Empty)
        {
            return Results.BadRequest("LessonId cannot be empty.");
        }

        if (request == null)
        {
            return Results.BadRequest("Request body cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Results.BadRequest("Title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return Results.BadRequest("Description cannot be empty.");
        }

        if (request.ContentSections == null || request.ContentSections.Count == 0)
        {
            return Results.BadRequest("ContentSections cannot be empty.");
        }

        try
        {
            var dbModel = await lessonService.UpdateLessonAsync(lessonId, request, ct);
            var response = dbModel.ToResponse();
            logger.LogInformation("Updated lesson {LessonId}", lessonId);
            return Results.Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            logger.LogWarning("Lesson {LessonId} not found for update", lessonId);
            return Results.NotFound($"Lesson {lessonId} not found");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while updating lesson {LessonId}", lessonId);
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error updating lesson {LessonId}", lessonId);
            return Results.Problem("An error occurred while updating the lesson.");
        }
    }

    private static async Task<IResult> DeleteLessonAsync(
        [FromRoute] Guid lessonId,
        [FromServices] ILessonService lessonService,
        [FromServices] ILogger<LessonsEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var _ = logger.BeginScope("Method={Method}, LessonId={LessonId}", nameof(DeleteLessonAsync), lessonId);

        if (lessonId == Guid.Empty)
        {
            return Results.BadRequest("LessonId cannot be empty.");
        }

        try
        {
            await lessonService.DeleteLessonAsync(lessonId, ct);
            logger.LogInformation("Deleted lesson {LessonId}", lessonId);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            logger.LogWarning("Lesson {LessonId} not found for deletion", lessonId);
            return Results.NotFound($"Lesson {lessonId} not found");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while deleting lesson {LessonId}", lessonId);
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error deleting lesson {LessonId}", lessonId);
            return Results.Problem("An error occurred while deleting the lesson.");
        }
    }
}
