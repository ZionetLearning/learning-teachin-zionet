using Manager.Models;
using Manager.Models.Games;

namespace Manager.Services.Clients.Accessor.Models;

/// <summary>
/// Response model received from Accessor service for game history
/// </summary>
public class GetHistoryAccessorResponse
{
    public PagedResult<SummaryHistoryDto>? Summary { get; set; }
    public PagedResult<AttemptHistoryDto>? Detailed { get; set; }

    public bool IsSummary => Summary is not null;
    public bool IsDetailed => Detailed is not null;
}
