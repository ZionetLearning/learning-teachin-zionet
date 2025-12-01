using Accessor.Models.Lessons;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Endpoints;

public static class LessonsEndpoints
{
    private sealed class LessonsEndpoint { }

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
        [FromServices] ILogger<LessonsEndpoint> logger,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            logger.LogWarning("GetLessonsByTeacherAsync called with empty UserId");
            return Results.BadRequest("UserId cannot be empty.");
        }

        using var scope = logger.BeginScope("GetLessonsByTeacherAsync. UserId={UserId}", userId);

        try
        {
            var lessons = await lessonService.GetLessonsByTeacherAsync(userId, ct);
            logger.LogInformation("Retrieved {Count} lessons for teacher {UserId}", lessons.Count, userId);
            return Results.Ok(lessons);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while retrieving lessons for teacher {UserId}", userId);
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving lessons for teacher {UserId}", userId);
            return Results.Problem("Error occurred while fetching lessons.", statusCode: 500);
        }
    }

    private static async Task<IResult> CreateLessonAsync(
        [FromBody] CreateLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ILogger<LessonsEndpoint> logger,
        CancellationToken ct)
    {
        if (request == null)
        {
            logger.LogWarning("CreateLessonAsync called with null request");
            return Results.BadRequest("Request body cannot be null.");
        }

        if (request.TeacherId == Guid.Empty)
        {
            logger.LogWarning("CreateLessonAsync called with empty TeacherId");
            return Results.BadRequest("TeacherId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            logger.LogWarning("CreateLessonAsync called with empty Title");
            return Results.BadRequest("Title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            logger.LogWarning("CreateLessonAsync called with empty Description");
            return Results.BadRequest("Description cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.ContentSectionsJson))
        {
            logger.LogWarning("CreateLessonAsync called with empty ContentSectionsJson");
            return Results.BadRequest("ContentSectionsJson cannot be empty.");
        }

        using var scope = logger.BeginScope("CreateLessonAsync. TeacherId={TeacherId}", request.TeacherId);

        try
        {
            var lesson = await lessonService.CreateLessonAsync(request, ct);
            logger.LogInformation("Created lesson {LessonId} for teacher {TeacherId}", lesson.LessonId, request.TeacherId);
            return Results.Created($"/lessons-accessor/{lesson.LessonId}", lesson);
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
            return Results.Problem("Unexpected error occurred while creating lesson.", statusCode: 500);
        }
    }

    private static async Task<IResult> UpdateLessonAsync(
        [FromRoute] Guid lessonId,
        [FromBody] UpdateLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ILogger<LessonsEndpoint> logger,
        CancellationToken ct)
    {
        if (lessonId == Guid.Empty)
        {
            logger.LogWarning("UpdateLessonAsync called with empty LessonId");
            return Results.BadRequest("LessonId cannot be empty.");
        }

        if (request == null)
        {
            logger.LogWarning("UpdateLessonAsync called with null request");
            return Results.BadRequest("Request body cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            logger.LogWarning("UpdateLessonAsync called with empty Title");
            return Results.BadRequest("Title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            logger.LogWarning("UpdateLessonAsync called with empty Description");
            return Results.BadRequest("Description cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.ContentSectionsJson))
        {
            logger.LogWarning("UpdateLessonAsync called with empty ContentSectionsJson");
            return Results.BadRequest("ContentSectionsJson cannot be empty.");
        }

        using var scope = logger.BeginScope("UpdateLessonAsync. LessonId={LessonId}", lessonId);

        try
        {
            var lesson = await lessonService.UpdateLessonAsync(lessonId, request, ct);
            logger.LogInformation("Updated lesson {LessonId}", lessonId);
            return Results.Ok(lesson);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            logger.LogWarning("Lesson {LessonId} not found for update", lessonId);
            return Results.NotFound(new { error = $"Lesson {lessonId} not found" });
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while updating lesson {LessonId}", lessonId);
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error updating lesson {LessonId}", lessonId);
            return Results.Problem("Unexpected error occurred while updating lesson.", statusCode: 500);
        }
    }

    private static async Task<IResult> DeleteLessonAsync(
        [FromRoute] Guid lessonId,
        [FromServices] ILessonService lessonService,
        [FromServices] ILogger<LessonsEndpoint> logger,
        CancellationToken ct)
    {
        if (lessonId == Guid.Empty)
        {
            logger.LogWarning("DeleteLessonAsync called with empty LessonId");
            return Results.BadRequest("LessonId cannot be empty.");
        }

        using var scope = logger.BeginScope("DeleteLessonAsync. LessonId={LessonId}", lessonId);

        try
        {
            await lessonService.DeleteLessonAsync(lessonId, ct);
            logger.LogInformation("Deleted lesson {LessonId}", lessonId);
            return Results.Ok(new { message = "Lesson deleted successfully" });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            logger.LogWarning("Lesson {LessonId} not found for deletion", lessonId);
            return Results.NotFound(new { error = $"Lesson {lessonId} not found" });
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while deleting lesson {LessonId}", lessonId);
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error deleting lesson {LessonId}", lessonId);
            return Results.Problem("Unexpected error occurred while deleting lesson.", statusCode: 500);
        }
    }
}
