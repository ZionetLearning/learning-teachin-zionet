namespace Engine.Models.Lessons;

public sealed class EngineLessonResponse
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required List<EngineContentSection> ContentSections { get; init; }
}

