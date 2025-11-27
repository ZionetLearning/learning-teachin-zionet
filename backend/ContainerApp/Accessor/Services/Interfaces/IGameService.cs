using Accessor.Models.Games;
using Accessor.Models.GameConfiguration;

namespace Accessor.Services.Interfaces;

public interface IGameService
{
    Task<SubmitAttemptResult> SubmitAttemptAsync(SubmitAttemptRequest request, CancellationToken ct);
    Task<GameHistoryResponse> GetHistoryAsync(Guid studentId, bool summary, int page, int pageSize, bool getPending, DateTimeOffset? fromDate, DateTimeOffset? toDate, CancellationToken ct);
    Task<PagedResult<MistakeDto>> GetMistakesAsync(Guid studentId, int page, int pageSize, DateTimeOffset? fromDate, DateTimeOffset? toDate, CancellationToken ct);
    Task<PagedResult<SummaryHistoryWithStudentDto>> GetAllHistoriesAsync(int page, int pageSize, CancellationToken ct);
    Task<List<AttemptedSentenceResult>> SaveGeneratedSentencesAsync(GeneratedSentenceDto dto, CancellationToken ct);
    Task<AttemptHistoryDto> GetAttemptDetailsAsync(Guid studentId, Guid attemptId, CancellationToken ct);
    Task DeleteAllGamesHistoryAsync(CancellationToken ct);
    Task<AttemptHistoryDto> GetLastAttemptAsync(Guid studentId, GameName gameType, CancellationToken ct);
}