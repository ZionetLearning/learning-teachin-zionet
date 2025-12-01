using Accessor.DB;
using Accessor.Models.Lessons;
using Accessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class LessonService : ILessonService
{
    private readonly AccessorDbContext _context;
    private readonly ILogger<LessonService> _logger;

    public LessonService(AccessorDbContext context, ILogger<LessonService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<LessonModel>> GetLessonsByTeacherAsync(Guid teacherId, CancellationToken ct)
    {
        try
        {
            return await _context.Lessons
                .Where(l => l.TeacherId == teacherId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lessons for teacher {TeacherId}", teacherId);
            throw;
        }
    }

    public async Task<LessonModel> CreateLessonAsync(CreateLessonRequest request, CancellationToken ct)
    {
        try
        {
            var lesson = new LessonModel
            {
                LessonId = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                ContentSectionsJson = request.ContentSectionsJson,
                TeacherId = request.TeacherId,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Created lesson {LessonId} for teacher {TeacherId}", lesson.LessonId, request.TeacherId);
            return lesson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lesson for teacher {TeacherId}", request.TeacherId);
            throw;
        }
    }

    public async Task<LessonModel> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request, CancellationToken ct)
    {
        try
        {
            var lesson = await _context.Lessons
                .SingleOrDefaultAsync(l => l.LessonId == lessonId, ct);

            if (lesson == null)
            {
                _logger.LogWarning("Lesson {LessonId} not found for update", lessonId);
                throw new InvalidOperationException($"Lesson {lessonId} not found");
            }

            lesson.Title = request.Title;
            lesson.Description = request.Description;
            lesson.ContentSectionsJson = request.ContentSectionsJson;
            lesson.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Updated lesson {LessonId}", lessonId);
            return lesson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lesson {LessonId}", lessonId);
            throw;
        }
    }

    public async Task DeleteLessonAsync(Guid lessonId, CancellationToken ct)
    {
        try
        {
            var lesson = await _context.Lessons
                .SingleOrDefaultAsync(l => l.LessonId == lessonId, ct);

            if (lesson == null)
            {
                _logger.LogWarning("Lesson {LessonId} not found for deletion", lessonId);
                throw new InvalidOperationException($"Lesson {lessonId} not found");
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Deleted lesson {LessonId}", lessonId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lesson {LessonId}", lessonId);
            throw;
        }
    }
}
