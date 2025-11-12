using Manager.Models.Games;
using Manager.Services.Clients.Accessor.Models;

namespace Manager.Services.Clients.Accessor;

public interface IGameAccessorClient
{
    Task<SubmitAttemptResult> SubmitAttemptAsync(Guid studentId, SubmitAttemptRequest request, CancellationToken ct = default);
    Task<GameHistoryResponse> GetHistoryAsync(Guid studentId, bool summary, int page, int pageSize, bool getPending, CancellationToken ct = default);
    Task<PagedResult<MistakeDto>> GetMistakesAsync(Guid studentId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<SummaryHistoryWithStudentDto>> GetAllHistoriesAsync(int page, int pageSize, CancellationToken ct = default);
    Task<List<AttemptedSentenceResult>> SaveGeneratedSentencesAsync(GeneratedSentenceDto dto, CancellationToken ct);
    Task<bool> DeleteAllGamesHistoryAsync(CancellationToken ct);
}
