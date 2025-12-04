using Manager.Models.Games;

namespace Manager.Services.Clients.Accessor.Models.Games;

/// <summary>
/// Response model received from Accessor service for all student histories
/// Matches Accessor's GetAllHistoriesResponse structure
/// </summary>
public sealed record GetAllHistoriesAccessorResponse
{
    public required IReadOnlyList<StudentExerciseHistory> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public bool HasNextPage => Page * PageSize < TotalCount;
}
