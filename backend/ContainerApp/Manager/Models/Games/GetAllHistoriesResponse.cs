namespace Manager.Models.Games;

/// <summary>
/// Response model for getting all student histories (admin/teacher view)
/// </summary>
public sealed record GetAllHistoriesResponse
{
    public required IReadOnlyList<StudentExerciseHistory> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
}
