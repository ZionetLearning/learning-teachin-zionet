namespace Accessor.Models.Games.Responses;

/// <summary>
/// Response model for getting all histories
/// </summary>
public sealed record GetAllHistoriesResponse
{
    public required IReadOnlyList<SummaryHistoryWithStudentResponseDto> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public bool HasNextPage => Page * PageSize < TotalCount;
}

