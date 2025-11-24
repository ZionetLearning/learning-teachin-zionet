using Accessor.Models.Games;

namespace Accessor.Services.Interfaces;

public interface IStudentPracticeHistoryService
{
    Task<PagedResult<SummaryHistoryWithStudentDto>> GetHistoryAsync(int page, int pageSize, CancellationToken ct);
}
