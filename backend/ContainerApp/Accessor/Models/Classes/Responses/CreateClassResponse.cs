namespace Accessor.Models.Classes.Responses;

/// <summary>
/// Response model for creating a class
/// </summary>
public sealed record CreateClassResponse
{
    public required Guid ClassId { get; init; }
    public required string Name { get; init; }
    public required string Code { get; init; }
    public string? Description { get; init; }
    public required DateTime CreatedAt { get; init; }
}

