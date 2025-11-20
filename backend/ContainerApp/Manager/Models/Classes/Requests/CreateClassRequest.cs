namespace Manager.Models.Classes.Requests;

/// <summary>
/// Request model for creating a class
/// </summary>
public sealed record CreateClassRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
