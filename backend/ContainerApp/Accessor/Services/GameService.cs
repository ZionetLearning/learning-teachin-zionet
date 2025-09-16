using Accessor.DB;
using Accessor.Services.Interfaces;
using Accessor.Models.Games;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class GameService : IGameService
{
    private readonly AccessorDbContext _db;
    private readonly ILogger<GameService> _logger;

    public GameService(AccessorDbContext db, ILogger<GameService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<SubmitAttemptResult> SubmitAttemptAsync(SubmitAttemptRequest request, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Submitting attempt. StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}, GivenAnswer={GivenAnswer}, CorrectAnswer={CorrectAnswer}", request.StudentId, request.GameType, request.Difficulty, string.Join(" ", request.GivenAnswer ?? new()), string.Join(" ", request.CorrectAnswer ?? new()));

            if (request.GivenAnswer is null || request.CorrectAnswer is null)
            {
                throw new ArgumentException("GivenAnswer and CorrectAnswer must not be null.");
            }

            // Compare directly
            var isCorrect = request.GivenAnswer.SequenceEqual(request.CorrectAnswer);

            // Find last attempt for this sentence
            var lastAttempt = await _db.GameAttempts
                .Where(a =>
                    a.StudentId == request.StudentId &&
                    a.GameType == request.GameType &&
                    a.Difficulty == request.Difficulty &&
                    a.CorrectAnswer.SequenceEqual(request.CorrectAnswer))
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(ct);

            int nextAttemptNumber;

            if (lastAttempt == null)
            {
                nextAttemptNumber = 1;
                _logger.LogDebug("No previous attempt found. Starting new sequence with AttemptNumber={AttemptNumber}", nextAttemptNumber);
            }
            else if (!lastAttempt.IsSuccess)
            {
                nextAttemptNumber = lastAttempt.AttemptNumber + 1;
                _logger.LogDebug("Previous attempt was incorrect. Incrementing AttemptNumber={AttemptNumber}", nextAttemptNumber);
            }
            else
            {
                nextAttemptNumber = 1;
                _logger.LogDebug("Previous attempt was success. Resetting AttemptNumber={AttemptNumber}", nextAttemptNumber);
            }

            var attempt = new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                StudentId = request.StudentId,
                GameType = request.GameType,
                Difficulty = request.Difficulty,
                CorrectAnswer = request.CorrectAnswer ?? new(),
                GivenAnswer = request.GivenAnswer ?? new(),
                IsSuccess = isCorrect,
                AttemptNumber = nextAttemptNumber,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.GameAttempts.Add(attempt);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Attempt saved. StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}, IsSuccess={IsSuccess}, AttemptNumber={AttemptNumber}", request.StudentId, request.GameType, request.Difficulty, isCorrect, nextAttemptNumber);

            return new SubmitAttemptResult
            {
                StudentId = request.StudentId,
                GameType = request.GameType,
                Difficulty = request.Difficulty,
                IsSuccess = isCorrect,
                CorrectAnswer = request.CorrectAnswer ?? new(),
                AttemptNumber = nextAttemptNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while submitting attempt. StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}", request.StudentId, request.GameType, request.Difficulty);
            throw;
        }
    }

    public async Task<IEnumerable<object>> GetHistoryAsync(Guid studentId, bool summary, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching history. StudentId={StudentId}, Summary={Summary}", studentId, summary);

            if (summary)
            {
                var summaryResult = await _db.GameAttempts
                    .Where(a => a.StudentId == studentId)
                    .GroupBy(a => new { a.GameType, a.Difficulty })
                    .Select(g => new SummaryHistoryDto
                    {
                        GameType = g.Key.GameType,
                        Difficulty = g.Key.Difficulty,
                        AttemptsCount = g.Count(),
                        TotalSuccesses = g.Count(x => x.IsSuccess),
                        TotalFailures = g.Count(x => !x.IsSuccess)
                    })
                    .ToListAsync(ct);

                _logger.LogInformation("Summary history retrieved. StudentId={StudentId}, Records={Count}", studentId, summaryResult.Count);
                return summaryResult;
            }
            else
            {
                var fullResult = await _db.GameAttempts
                    .Where(a => a.StudentId == studentId)
                    .Select(a => new AttemptHistoryDto
                    {
                        AttemptId = a.AttemptId,
                        GameType = a.GameType,
                        Difficulty = a.Difficulty,
                        GivenAnswer = a.GivenAnswer,
                        CorrectAnswer = a.CorrectAnswer,
                        IsSuccess = a.IsSuccess,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync(ct);

                _logger.LogInformation("Full history retrieved. StudentId={StudentId}, Records={Count}", studentId, fullResult.Count);
                return fullResult;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching history. StudentId={StudentId}, Summary={Summary}", studentId, summary);
            throw;
        }
    }

    public async Task<IEnumerable<MistakeDto>> GetMistakesAsync(Guid studentId, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching mistakes. StudentId={StudentId}", studentId);

            var attempts = await _db.GameAttempts
                .Where(a => a.StudentId == studentId)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync(ct);

            // Group by (GameType + Difficulty + CorrectAnswer)
            var grouped = attempts
                .GroupBy(a => new { a.GameType, a.Difficulty, CorrectKey = string.Join(" ", a.CorrectAnswer) });

            var mistakes = grouped
                .Where(g => !g.Any(x => x.IsSuccess)) // only groups with NO successes
                .Select(g => new MistakeDto
                {
                    GameType = g.Key.GameType,
                    Difficulty = g.Key.Difficulty,
                    // last given answer attempt (or you can collect all wrongs)
                    LastWrongAnswer = g.Last().GivenAnswer
                })
                .ToList();

            _logger.LogInformation("Mistakes retrieved. StudentId={StudentId}, Count={Count}", studentId, mistakes.Count);
            return mistakes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching mistakes. StudentId={StudentId}", studentId);
            throw;
        }
    }

    public async Task<IEnumerable<SummaryHistoryWithStudentDto>> GetAllHistoriesAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching all histories (summary for all students)");

            var result = await _db.GameAttempts
                .GroupBy(a => new { a.StudentId, a.GameType, a.Difficulty })
                .Select(g => new SummaryHistoryWithStudentDto
                {
                    StudentId = g.Key.StudentId,
                    GameType = g.Key.GameType,
                    Difficulty = g.Key.Difficulty,
                    AttemptsCount = g.Count(),
                    TotalSuccesses = g.Count(x => x.IsSuccess),
                    TotalFailures = g.Count(x => !x.IsSuccess)
                })
                .ToListAsync(ct);

            _logger.LogInformation("All histories retrieved. Records={Count}", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all histories");
            throw;
        }
    }
}