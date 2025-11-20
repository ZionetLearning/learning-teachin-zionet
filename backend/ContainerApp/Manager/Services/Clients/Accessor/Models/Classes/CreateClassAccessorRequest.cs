namespace Manager.Services.Clients.Accessor.Models.Classes;

/// <summary>
/// Request model sent to Accessor service for creating a class
/// </summary>
public sealed record CreateClassAccessorRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
