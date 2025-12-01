using Accessor.Models.Lessons;

namespace Accessor.Services.Interfaces;

public interface ILessonService
{
    Task<IReadOnlyList<LessonModel>> GetLessonsByTeacherAsync(Guid teacherId, CancellationToken ct);
    Task<LessonModel> CreateLessonAsync(CreateLessonRequest request, CancellationToken ct);
    Task<LessonModel> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request, CancellationToken ct);
    Task DeleteLessonAsync(Guid lessonId, CancellationToken ct);
}
