using Engine.Models.Lessons;

namespace Engine.Services;

public interface ILessonGeneratorService
{
    Task<EngineLessonResponse> GenerateLessonAsync(EngineLessonRequest request, CancellationToken ct = default);
}

