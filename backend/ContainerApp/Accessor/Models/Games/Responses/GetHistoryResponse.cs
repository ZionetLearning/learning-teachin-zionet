namespace Accessor.Models.Games.Responses;

/// <summary>
/// Response model for game history that can contain either summary or detailed data
/// </summary>
public sealed record GetHistoryResponse
{
    public PagedResponseResult<SummaryHistoryResponseDto>? Summary { get; init; }
    public PagedResponseResult<AttemptHistoryResponseDto>? Detailed { get; init; }

    public bool IsSummary => Summary is not null;
    public bool IsDetailed => Detailed is not null;
}

