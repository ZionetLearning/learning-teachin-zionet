namespace Accessor.Models.Games.Responses;

/// <summary>
/// Response model for getting mistakes
/// </summary>
public sealed record GetMistakesResponse
{
    public required IReadOnlyList<MistakeResponseDto> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public bool HasNextPage => Page * PageSize < TotalCount;
}

