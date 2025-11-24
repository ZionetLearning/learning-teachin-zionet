namespace Manager.Models.Tasks.Responses;

/// <summary>
/// Response model for creating a task
/// </summary>
public sealed record CreateTaskResponse
{
    public required int Id { get; init; }
    public required string Status { get; init; }
}
