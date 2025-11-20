namespace Manager.Models.Classes.Responses;

using System.Collections.Generic;
/// <summary>
/// Response model for getting user's classes
/// </summary>
public sealed record GetMyClassesResponse
{
    public required IReadOnlyList<ClassSummaryDto> Classes { get; init; }
}
