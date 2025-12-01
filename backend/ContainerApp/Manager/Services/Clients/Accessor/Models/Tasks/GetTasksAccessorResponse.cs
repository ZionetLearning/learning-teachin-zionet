namespace Manager.Services.Clients.Accessor.Models.Tasks;

/// <summary>
/// Response model received from Accessor service for getting all tasks
/// </summary>
public sealed record GetTasksAccessorResponse
{
    public required IReadOnlyList<TaskSummaryAccessorDto> Tasks { get; init; }
}

/// <summary>
/// DTO for task summary information from Accessor
/// </summary>
public sealed record TaskSummaryAccessorDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
