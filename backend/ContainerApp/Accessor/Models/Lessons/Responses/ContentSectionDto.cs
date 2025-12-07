namespace Accessor.Models.Lessons.Responses;

public sealed record ContentSectionDto
{
    public required string Heading { get; init; }
    public required string Body { get; init; }
}

