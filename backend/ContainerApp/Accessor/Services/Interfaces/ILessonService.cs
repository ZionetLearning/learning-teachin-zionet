using Accessor.Models.Lessons;
using Accessor.Models.Lessons.Requests;

namespace Accessor.Services.Interfaces;

public interface ILessonService
{
    Task<IReadOnlyList<Lesson>> GetLessonsByTeacherAsync(Guid teacherId, CancellationToken ct);
    Task<Lesson> CreateLessonAsync(CreateLessonRequest request, CancellationToken ct);
    Task<Lesson> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request, CancellationToken ct);
    Task DeleteLessonAsync(Guid lessonId, CancellationToken ct);
}
