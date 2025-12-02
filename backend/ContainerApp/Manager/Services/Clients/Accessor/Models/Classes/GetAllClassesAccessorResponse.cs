namespace Manager.Services.Clients.Accessor.Models.Classes;

/// <summary>
/// Response model received from Accessor service for all classes
/// </summary>
public sealed record GetAllClassesAccessorResponse
{
    public required IReadOnlyList<ClassAccessorDto> Classes { get; init; }
}

/// <summary>
/// DTO representing a class from Accessor
/// </summary>
public sealed record ClassAccessorDto
{
    public required Guid ClassId { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<MemberAccessorDto> Members { get; init; }
}
