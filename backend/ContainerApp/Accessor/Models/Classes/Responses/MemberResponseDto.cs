namespace Accessor.Models.Classes.Responses;

/// <summary>
/// DTO representing a class member in response
/// </summary>
public sealed record MemberResponseDto
{
    public required Guid MemberId { get; init; }
    public required string Name { get; init; }
    public required int Role { get; init; }
}

