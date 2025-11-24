namespace Manager.Services.Clients.Accessor.Models.Classes;

/// <summary>
/// Response model received from Accessor service for user's classes
/// </summary>
public sealed record GetMyClassesAccessorResponse
{
    public required IReadOnlyList<ClassAccessorDto> Classes { get; init; }
}
