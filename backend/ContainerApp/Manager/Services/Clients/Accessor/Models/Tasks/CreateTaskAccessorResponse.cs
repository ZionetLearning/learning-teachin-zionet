namespace Manager.Services.Clients.Accessor.Models.Tasks;

/// <summary>
/// Response model received from Accessor service after creating a task
/// </summary>
public sealed record CreateTaskAccessorResponse
{
    public required bool Success { get; init; }
    public required string Message { get; init; }
    public required int Id { get; init; }
}
