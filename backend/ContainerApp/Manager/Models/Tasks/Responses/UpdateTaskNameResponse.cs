namespace Manager.Models.Tasks.Responses;

/// <summary>
/// Response model for updating a task name
/// </summary>
public sealed record UpdateTaskNameResponse
{
    public required string Message { get; init; }
}
