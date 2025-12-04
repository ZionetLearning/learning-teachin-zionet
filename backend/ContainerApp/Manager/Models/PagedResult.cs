namespace Manager.Models;

public sealed record PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasNextPage { get; set; }
}
