namespace Manager.Models.Classes.Responses;

/// <summary>
/// Response model for getting a single class
/// </summary>
public sealed record GetClassResponse
{
    public required Guid ClassId { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<ClassMemberDto> Members { get; init; }
}

/// <summary>
/// DTO representing a class member
/// </summary>
public sealed record ClassMemberDto
{
    public required Guid MemberId { get; init; }
    public required string Name { get; init; }
    public required int Role { get; init; }
}
