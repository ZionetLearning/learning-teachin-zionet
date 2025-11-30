namespace Manager.Models.Tasks.Responses;

/// <summary>
/// Response model for getting a single task
/// </summary>
public sealed record GetTaskResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Payload { get; init; }
}
