namespace Manager.Models.Games;

/// <summary>
/// Response model for game history endpoint (can contain either summary or detailed data)
/// </summary>
public sealed record GetHistoryResponse
{
    public PagedResult<SummaryHistoryDto>? Summary { get; init; }
    public PagedResult<AttemptHistoryDto>? Detailed { get; init; }

    public bool IsSummary => Summary is not null;
    public bool IsDetailed => Detailed is not null;
}
