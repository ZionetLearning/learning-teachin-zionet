namespace Manager.Models.Games;

/// <summary>
/// Wrapper for game history response that can contain either summary or detailed data
/// </summary>
public sealed record GameHistoryResponse
{
    public PagedResult<SummaryHistoryDto>? Summary { get; init; }
    public PagedResult<AttemptHistoryDto>? Detailed { get; init; }

    public bool IsSummary => Summary is not null;
    public bool IsDetailed => Detailed is not null;
}
