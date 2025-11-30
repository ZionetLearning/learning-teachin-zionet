namespace Manager.Models.Tasks;

/// <summary>
/// DTO for task summary information
/// </summary>
public sealed record TaskSummaryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
