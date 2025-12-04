using Manager.Models.Users;

namespace Manager.Services.Clients.Accessor.Models.Classes;

/// <summary>
/// Response model received from Accessor service for a single class
/// </summary>
public sealed record GetClassAccessorResponse
{
    public required Guid ClassId { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<MemberAccessorDto> Members { get; init; }
}

/// <summary>
/// DTO representing a class member from Accessor
/// </summary>
public sealed record MemberAccessorDto
{
    public required Guid MemberId { get; init; }
    public required string Name { get; init; }
    public required Role Role { get; init; }
}
