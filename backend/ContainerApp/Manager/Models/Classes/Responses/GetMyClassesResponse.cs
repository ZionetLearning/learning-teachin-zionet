namespace Manager.Models.Classes.Responses;

/// <summary>
/// Response model for getting user's classes
/// </summary>
public sealed record GetMyClassesResponse
{
    public required List<ClassSummaryDto> Classes { get; init; }
}
