namespace Manager.Models.Games;

/// <summary>
/// Response model for getting student mistakes
/// </summary>
public sealed record GetMistakesResponse
{
    public required IEnumerable<ExerciseMistakes> Items { get; set; } = new List<ExerciseMistakes>();
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalCount { get; set; }
}
