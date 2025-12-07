namespace Accessor.Models.Lessons.Requests;

public sealed record UpdateLessonRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required List<ContentSection> ContentSections { get; init; }
}

