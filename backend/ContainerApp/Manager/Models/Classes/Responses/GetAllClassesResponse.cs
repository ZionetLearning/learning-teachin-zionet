namespace Manager.Models.Classes.Responses;

/// <summary>
/// Response model for getting all classes
/// </summary>
public sealed record GetAllClassesResponse
{
    public required IReadOnlyList<ClassSummaryDto> Classes { get; init; }
}

/// <summary>
/// Summary DTO for a class
/// </summary>
public sealed record ClassSummaryDto
{
    public required Guid ClassId { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<ClassMemberDto> Members { get; init; }
}
