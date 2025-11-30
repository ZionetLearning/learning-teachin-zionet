namespace Accessor.Models.Classes.Responses;

/// <summary>
/// Response model for getting all classes
/// </summary>
public sealed record GetAllClassesResponse
{
    public required IReadOnlyList<ClassSummaryDto> Classes { get; init; }
}

/// <summary>
/// DTO representing a class summary
/// </summary>
public sealed record ClassSummaryDto
{
    public required Guid ClassId { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<MemberResponseDto> Members { get; init; }
}

