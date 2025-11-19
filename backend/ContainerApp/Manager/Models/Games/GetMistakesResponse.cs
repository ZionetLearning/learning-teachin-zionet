namespace Manager.Models.Games;

/// <summary>
/// Response model for getting student mistakes
/// </summary>
public sealed record GetMistakesResponse
{
    public required IEnumerable<ExerciseMistakes> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
}
