using Accessor.Models.Games;

namespace Accessor.Services.Interfaces;

public interface IGameService
{
    Task<SubmitAttemptResult> SubmitAttemptAsync(SubmitAttemptRequest request, CancellationToken ct);
    Task<IEnumerable<object>> GetHistoryAsync(Guid studentId, bool summary, CancellationToken ct);
    Task<IEnumerable<MistakeDto>> GetMistakesAsync(Guid studentId, CancellationToken ct);
    Task<IEnumerable<SummaryHistoryWithStudentDto>> GetAllHistoriesAsync(CancellationToken ct);
}