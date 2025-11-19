namespace Manager.Models.Games;

/// <summary>
/// Response model for getting student mistakes
/// </summary>
public sealed class GetMistakesResponse
{
    public required IEnumerable<MistakeDto> Items { get; set; } = new List<MistakeDto>();
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalCount { get; set; }
}
