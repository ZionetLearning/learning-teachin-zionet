using Manager.Models;
using Manager.Models.Games;

namespace Manager.Services.Clients.Accessor.Models.Games;

/// <summary>
/// Response model received from Accessor service for game history
/// </summary>
public sealed record GetHistoryAccessorResponse
{
    public PagedResult<SummaryHistoryDto>? Summary { get; init; }
    public PagedResult<AttemptHistoryDto>? Detailed { get; init; }

    public bool IsSummary => Summary is not null;
    public bool IsDetailed => Detailed is not null;
}
