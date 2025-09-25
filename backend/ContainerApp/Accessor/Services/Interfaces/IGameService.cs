using Accessor.Models.Games;

namespace Accessor.Services.Interfaces;

public interface IGameService
{
    Task<SubmitAttemptResult> SubmitAttemptAsync(SubmitAttemptRequest request, CancellationToken ct);
    Task<PagedResult<object>> GetHistoryAsync(Guid studentId, bool summary, int page, int pageSize, CancellationToken ct);
    Task<PagedResult<MistakeDto>> GetMistakesAsync(Guid studentId, int page, int pageSize, CancellationToken ct);
    Task<PagedResult<SummaryHistoryWithStudentDto>> GetAllHistoriesAsync(int page, int pageSize, CancellationToken ct);
    Task<Guid> SaveGeneratedSentenceAsync(GeneratedSentenceDto dto, CancellationToken ct);
}