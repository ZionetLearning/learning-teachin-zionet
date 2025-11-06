using Dapr.Client;
using Manager.Constants;
using Manager.Models.Games;
using Manager.Services.Clients.Accessor.Models;

namespace Manager.Services.Clients.Accessor;

public class GameAccessorClient : IGameAccessorClient
{
    private readonly ILogger<GameAccessorClient> _logger;
    private readonly DaprClient _daprClient;

    public GameAccessorClient(ILogger<GameAccessorClient> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    public async Task<SubmitAttemptResult> SubmitAttemptAsync(Guid studentId, SubmitAttemptRequest request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Forwarding SubmitAttempt to Accessor. StudentId={StudentId}, ExerciseId={ExerciseId}", studentId, request.ExerciseId);

            var accessorRequest = new SubmitAttemptRequestDto
            {
                StudentId = studentId,
                ExerciseId = request.ExerciseId,
                GivenAnswer = request.GivenAnswer
            };

            var result = await _daprClient.InvokeMethodAsync<SubmitAttemptRequestDto, SubmitAttemptResult>(
                HttpMethod.Post, AppIds.Accessor, "games-accessor/attempt", accessorRequest, ct
            );

            _logger.LogInformation("Received SubmitAttemptResult from Accessor. StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}, Status={Status}, AttemptNumber={AttemptNumber}",
                result.StudentId, result.GameType, result.Difficulty, result.Status, result.AttemptNumber);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to forward SubmitAttempt to Accessor. StudentId={StudentId}, ExerciseId={ExerciseId}", studentId, request.ExerciseId);
            throw;
        }
    }

    public async Task<GameHistoryResponse> GetHistoryAsync(Guid studentId, bool summary, int page, int pageSize, bool getPending, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Requesting history from Accessor. StudentId={StudentId}, Summary={Summary}, Page={Page}, PageSize={PageSize}, GetPending={GetPending}",
                studentId, summary, page, pageSize, getPending);

            if (summary)
            {
                var summaryResult = await _daprClient.InvokeMethodAsync<PagedResult<SummaryHistoryDto>>(
                    HttpMethod.Get,
                    AppIds.Accessor,
                    $"games-accessor/history/{studentId}?summary=true&page={page}&pageSize={pageSize}&getPending={getPending}",
                    cancellationToken: ct
                );

                if (summaryResult == null)
                {
                    _logger.LogWarning("Accessor returned null summary history. StudentId={StudentId}", studentId);
                    summaryResult = new PagedResult<SummaryHistoryDto> { Page = page, PageSize = pageSize, TotalCount = 0 };
                }

                _logger.LogInformation("Received summary history from Accessor. StudentId={StudentId}, Items={Count}, TotalCount={TotalCount}",
                    studentId, summaryResult.Items.Count(), summaryResult.TotalCount);

                return new GameHistoryResponse { Summary = summaryResult };
            }
            else
            {
                var detailedResult = await _daprClient.InvokeMethodAsync<PagedResult<AttemptHistoryDto>>(
                    HttpMethod.Get,
                    AppIds.Accessor,
                    $"games-accessor/history/{studentId}?summary=false&page={page}&pageSize={pageSize}&getPending={getPending}",
                    cancellationToken: ct
                );

                if (detailedResult == null)
                {
                    _logger.LogWarning("Accessor returned null detailed history. StudentId={StudentId}", studentId);
                    detailedResult = new PagedResult<AttemptHistoryDto> { Page = page, PageSize = pageSize, TotalCount = 0 };
                }

                _logger.LogInformation("Received detailed history from Accessor. StudentId={StudentId}, Items={Count}, TotalCount={TotalCount}",
                    studentId, detailedResult.Items.Count(), detailedResult.TotalCount);

                return new GameHistoryResponse { Detailed = detailedResult };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get history from Accessor. StudentId={StudentId}, Summary={Summary}, GetPending={GetPending}", studentId, summary, getPending);

            if (summary)
            {
                return new GameHistoryResponse
                {
                    Summary = new PagedResult<SummaryHistoryDto> { Page = page, PageSize = pageSize, TotalCount = 0 }
                };
            }
            else
            {
                return new GameHistoryResponse
                {
                    Detailed = new PagedResult<AttemptHistoryDto> { Page = page, PageSize = pageSize, TotalCount = 0 }
                };
            }
        }
    }

    public async Task<PagedResult<MistakeDto>> GetMistakesAsync(Guid studentId, int page, int pageSize, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Requesting mistakes from Accessor. StudentId={StudentId}, Page={Page}, PageSize={PageSize}", studentId, page, pageSize);

            var result = await _daprClient.InvokeMethodAsync<PagedResult<MistakeDto>>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"games-accessor/mistakes/{studentId}?page={page}&pageSize={pageSize}",
                cancellationToken: ct
            );

            if (result == null)
            {
                _logger.LogWarning("Accessor returned null mistakes. StudentId={StudentId}", studentId);
                return new PagedResult<MistakeDto> { Page = page, PageSize = pageSize, TotalCount = 0 };
            }

            _logger.LogInformation("Received mistakes from Accessor. StudentId={StudentId}, Items={Count}, TotalCount={TotalCount}",
                studentId, result.Items.Count(), result.TotalCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get mistakes from Accessor. StudentId={StudentId}", studentId);
            return new PagedResult<MistakeDto> { Page = page, PageSize = pageSize, TotalCount = 0 };
        }
    }

    public async Task<PagedResult<SummaryHistoryWithStudentDto>> GetAllHistoriesAsync(int page, int pageSize, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Requesting all histories from Accessor. Page={Page}, PageSize={PageSize}", page, pageSize);

            var result = await _daprClient.InvokeMethodAsync<PagedResult<SummaryHistoryWithStudentDto>>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"games-accessor/all-history?page={page}&pageSize={pageSize}",
                cancellationToken: ct
            );

            if (result == null)
            {
                _logger.LogWarning("Accessor returned null for all histories.");
                return new PagedResult<SummaryHistoryWithStudentDto> { Page = page, PageSize = pageSize, TotalCount = 0 };
            }

            _logger.LogInformation("Received all histories from Accessor. Items={Count}, TotalCount={TotalCount}", result.Items.Count(), result.TotalCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all histories from Accessor");
            return new PagedResult<SummaryHistoryWithStudentDto> { Page = page, PageSize = pageSize, TotalCount = 0 };
        }
    }

    public async Task<List<AttemptedSentenceResult>> SaveGeneratedSentencesAsync(GeneratedSentenceDto dto, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Saving generated sentence for StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}",
                dto.StudentId, dto.GameType, dto.Difficulty);

            var result = await _daprClient.InvokeMethodAsync<GeneratedSentenceDto, List<AttemptedSentenceResult>>(
                HttpMethod.Post,
                AppIds.Accessor,
                "games-accessor/generated-sentences",
                dto,
                cancellationToken: ct
            );

            _logger.LogInformation("Generated sentence saved for StudentId={StudentId}", dto.StudentId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save generated sentence for StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}",
                dto.StudentId, dto.GameType, dto.Difficulty);
            throw;
        }
    }

    public async Task<bool> DeleteAllGamesHistoryAsync(CancellationToken ct)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Delete,
                AppIds.Accessor,
                $"/games-accessor/all-history",
                ct);
            _logger.LogInformation("All games history deleted successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while deleting all games history.");
            return false;
        }
    }
}
