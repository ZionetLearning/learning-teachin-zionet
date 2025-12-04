using Accessor.Models.GameConfiguration;
using Accessor.Models.Games;
using Accessor.Models.Games.Requests;

namespace Accessor.Services.Interfaces;

/// <summary>
/// Game service interface - works with DB models and internal DTOs only
/// </summary>
public interface IGameService
{
    Task<GameAttempt> SubmitAttemptAsync(SubmitAttemptRequest request, CancellationToken ct);
    Task<GameHistoryDto> GetHistoryAsync(Guid studentId, bool summary, int page, int pageSize, bool getPending, DateTimeOffset? fromDate, DateTimeOffset? toDate, CancellationToken ct);
    Task<PagedResult<MistakeDto>> GetMistakesAsync(Guid studentId, int page, int pageSize, DateTimeOffset? fromDate, DateTimeOffset? toDate, CancellationToken ct);
    Task<PagedResult<SummaryHistoryWithStudentDto>> GetAllHistoriesAsync(int page, int pageSize, CancellationToken ct);
    Task<List<GeneratedSentenceResultDto>> SaveGeneratedSentencesAsync(SaveGeneratedSentencesRequest request, CancellationToken ct);
    Task<AttemptHistoryDto> GetAttemptDetailsAsync(Guid studentId, Guid attemptId, CancellationToken ct);
    Task DeleteAllGamesHistoryAsync(CancellationToken ct);
    Task<AttemptHistoryDto> GetLastAttemptAsync(Guid studentId, GameName gameType, CancellationToken ct);
}
