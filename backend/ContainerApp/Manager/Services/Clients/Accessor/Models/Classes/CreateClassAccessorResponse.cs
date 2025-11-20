namespace Manager.Services.Clients.Accessor.Models.Classes;

/// <summary>
/// Response model received from Accessor service for creating a class
/// </summary>
public sealed record CreateClassAccessorResponse
{
    public required Guid ClassId { get; init; }
    public required string Name { get; init; }
    public required string Code { get; init; }
    public string? Description { get; init; }
    public required DateTime CreatedAt { get; init; }
}
