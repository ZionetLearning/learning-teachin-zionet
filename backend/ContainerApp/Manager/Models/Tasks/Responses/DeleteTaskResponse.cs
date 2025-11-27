namespace Manager.Models.Tasks.Responses;

/// <summary>
/// Response model for deleting a task
/// </summary>
public sealed record DeleteTaskResponse
{
    public required string Message { get; init; }
}
