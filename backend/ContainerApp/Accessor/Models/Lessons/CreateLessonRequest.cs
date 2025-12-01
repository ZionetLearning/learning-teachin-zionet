namespace Accessor.Models.Lessons;

public sealed class CreateLessonRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string ContentSectionsJson { get; init; }
    public required Guid TeacherId { get; init; }
}
