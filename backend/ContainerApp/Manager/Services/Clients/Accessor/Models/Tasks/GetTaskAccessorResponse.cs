namespace Manager.Services.Clients.Accessor.Models.Tasks;

/// <summary>
/// Response model received from Accessor service for getting a single task
/// </summary>
public sealed record GetTaskAccessorResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Payload { get; init; }
}
