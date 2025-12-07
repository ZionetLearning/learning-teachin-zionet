namespace Accessor.Models.Lessons.Responses;

public sealed record LessonResponse
{
    public required Guid LessonId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyList<ContentSectionDto> ContentSections { get; init; }
    public required Guid TeacherId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime ModifiedAt { get; init; }
}

