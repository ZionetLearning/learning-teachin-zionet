namespace Manager.Services.Clients.Accessor.Models.Tasks;

/// <summary>
/// Request model sent to Accessor service for updating a task name
/// </summary>
public sealed record UpdateTaskNameAccessorRequest
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
