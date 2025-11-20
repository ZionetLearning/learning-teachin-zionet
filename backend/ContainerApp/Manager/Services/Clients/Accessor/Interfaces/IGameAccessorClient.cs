using Manager.Models.Games;
using Manager.Services.Clients.Accessor.Models;
using Manager.Services.Clients.Accessor.Models.Games;

namespace Manager.Services.Clients.Accessor.Interfaces;

public interface IGameAccessorClient
{
    Task<SubmitAttemptAccessorResponse> SubmitAttemptAsync(Guid studentId, SubmitAttemptRequest request, CancellationToken ct = default);
    Task<GetHistoryAccessorResponse> GetHistoryAsync(Guid studentId, bool summary, int page, int pageSize, bool getPending, CancellationToken ct = default);
    Task<GetMistakesAccessorResponse> GetMistakesAsync(Guid studentId, int page, int pageSize, CancellationToken ct = default);
    Task<GetAllHistoriesAccessorResponse> GetAllHistoriesAsync(int page, int pageSize, CancellationToken ct = default);
    Task<List<AttemptedSentenceResult>> SaveGeneratedSentencesAsync(GeneratedSentenceDto dto, CancellationToken ct);
    Task<bool> DeleteAllGamesHistoryAsync(CancellationToken ct);
}
