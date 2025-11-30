namespace Accessor.Models.Lessons;

public sealed class UpdateLessonRequest
{
    public required Guid LessonId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string ContentSectionsJson { get; init; }
}
