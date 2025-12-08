using Manager.Models.Games;

namespace Manager.Services.Clients.Accessor.Models.Games;

/// <summary>
/// Response model received from Accessor service for game history
/// Matches Accessor's GetHistoryResponse structure
/// </summary>
public sealed record GetHistoryAccessorResponse
{
    public PagedResponseResult<SummaryHistoryDto>? Summary { get; init; }
    public PagedResponseResult<AttemptHistoryDto>? Detailed { get; init; }

    public bool IsSummary => Summary is not null;
    public bool IsDetailed => Detailed is not null;
}

/// <summary>
/// Paged result matching Accessor's PagedResponseResult structure
/// </summary>
public sealed record PagedResponseResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public bool HasNextPage => Page * PageSize < TotalCount;
}
