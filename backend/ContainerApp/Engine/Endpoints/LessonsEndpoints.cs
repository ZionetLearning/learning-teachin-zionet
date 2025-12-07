using Engine.Models.Lessons;
using Engine.Services;
using Microsoft.AspNetCore.Mvc;

namespace Engine.Endpoints;

public static class LessonsEndpoints
{
    private sealed class LessonsEndpoint { }

    public static WebApplication MapLessonsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/lessons-engine").WithTags("Lessons");

        group.MapPost("/generate", GenerateLessonAsync)
            .WithName("GenerateLesson");

        return app;
    }

    private static async Task<IResult> GenerateLessonAsync(
        [FromBody] EngineLessonRequest request,
        [FromServices] ILessonGeneratorService lessonGenerator,
        [FromServices] ILogger<LessonsEndpoint> logger,
        CancellationToken ct)
    {
        if (request is null)
        {
            logger.LogWarning("GenerateLessonAsync called with null request");
            return Results.BadRequest(new { error = "Request body cannot be null" });
        }

        if (string.IsNullOrWhiteSpace(request.Topic))
        {
            logger.LogWarning("GenerateLessonAsync called with empty topic");
            return Results.BadRequest(new { error = "Topic cannot be empty" });
        }

        using var scope = logger.BeginScope("GenerateLessonAsync. Topic={Topic}", request.Topic);

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(30));

            var lesson = await lessonGenerator.GenerateLessonAsync(request, cts.Token);
            logger.LogInformation("Successfully generated lesson: {Title}", lesson.Title);
            return Results.Ok(lesson);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Lesson generation timed out for topic: {Topic}", request.Topic);
            return Results.Problem(
                detail: "Lesson generation timed out. Please try again.",
                statusCode: 504);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled while generating lesson for topic: {Topic}", request.Topic);
            return Results.StatusCode(499);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Invalid operation while generating lesson for topic: {Topic}", request.Topic);
            return Results.Problem(
                detail: ex.Message,
                statusCode: 422);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error generating lesson for topic: {Topic}", request.Topic);
            return Results.Problem(
                detail: "An unexpected error occurred while generating the lesson.",
                statusCode: 500);
        }
    }
}

