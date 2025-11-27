namespace Accessor.Models.Classes.Responses;

/// <summary>
/// Response model for getting a single class
/// </summary>
public sealed record GetClassResponse
{
    public required Guid ClassId { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<MemberResponseDto> Members { get; init; }
}

