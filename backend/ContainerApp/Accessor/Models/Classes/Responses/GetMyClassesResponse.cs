namespace Accessor.Models.Classes.Responses;

/// <summary>
/// Response model for getting classes for a user
/// </summary>
public sealed record GetMyClassesResponse
{
    public required IReadOnlyList<ClassSummaryDto> Classes { get; init; }
}

