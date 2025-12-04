namespace Accessor.Models.Games.Responses;

/// <summary>
/// Generic paged result for API responses
/// </summary>
public sealed record PagedResponseResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public bool HasNextPage => Page * PageSize < TotalCount;
}

