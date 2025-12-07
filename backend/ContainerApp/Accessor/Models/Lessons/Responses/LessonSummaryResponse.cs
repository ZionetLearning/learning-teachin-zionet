namespace Accessor.Models.Lessons.Responses;

public sealed record LessonSummaryResponse
{
    public required Guid LessonId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required Guid TeacherId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime ModifiedAt { get; init; }
}

