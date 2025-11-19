namespace Manager.Models.Games;

/// <summary>
/// Response model for getting all student histories (admin/teacher view)
/// </summary>
public sealed record GetAllHistoriesResponse
{
    public required IEnumerable<StudentExerciseHistory> Items { get; set; } = new List<StudentExerciseHistory>();
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalCount { get; set; }
}
