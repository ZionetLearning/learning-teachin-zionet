namespace Manager.Services.Clients.Accessor.Models.Tasks;

/// <summary>
/// Request model sent to Accessor service for creating a task
/// </summary>
public sealed record CreateTaskAccessorRequest
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Payload { get; init; }
}
