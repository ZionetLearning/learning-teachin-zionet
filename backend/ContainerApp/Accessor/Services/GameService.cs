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
            _logger.LogInformation("Submitting attempt. StudentId={StudentId}, ExerciseId={ExerciseId}, GivenAnswer={GivenAnswer}", request.StudentId, request.ExerciseId, string.Join(" ", request.GivenAnswer ?? new()));

            if (request.GivenAnswer is null)
            {
                throw new ArgumentException("GivenAnswer must not be null.");
            }

            // Check if its retry attempt
            var retryAttempt = await _db.GameAttempts
                .FirstOrDefaultAsync(a => a.AttemptId == request.ExerciseId && a.Status == AttemptStatus.Failure, ct);

            if (retryAttempt is not null)
            {
                // Retry: Create a new attempt based on the failed one

                var isCorrectAns = request.GivenAnswer.SequenceEqual(retryAttempt.CorrectAnswer);

                // Get latest attempt number for this exercise
                var lastAttemptDB = await _db.GameAttempts
                    .Where(a => a.StudentId == retryAttempt.StudentId &&
                                a.ExerciseId == retryAttempt.ExerciseId &&
                                a.Status != AttemptStatus.Pending)
                    .OrderByDescending(a => a.AttemptNumber)
                    .FirstOrDefaultAsync(ct);

                var AttemptNumber = (lastAttemptDB?.AttemptNumber ?? 0) + 1;

                var newRetryAttempt = new GameAttempt
                {
                    AttemptId = Guid.NewGuid(),
                    ExerciseId = retryAttempt.ExerciseId,
                    StudentId = retryAttempt.StudentId,
                    GameType = retryAttempt.GameType,
                    Difficulty = retryAttempt.Difficulty,
                    CorrectAnswer = retryAttempt.CorrectAnswer,
                    GivenAnswer = request.GivenAnswer,
                    Status = isCorrectAns ? AttemptStatus.Success : AttemptStatus.Failure,
                    AttemptNumber = AttemptNumber,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _db.GameAttempts.Add(newRetryAttempt);
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Retry created as new attempt. NewAttemptId={AttemptId}, OriginalAttemptId={OriginalId}, AttemptNumber={AttemptNumber}, Status={Status}",
                    newRetryAttempt.AttemptId, newRetryAttempt.AttemptId, newRetryAttempt.AttemptNumber, newRetryAttempt.Status);

                return new SubmitAttemptResult
                {
                    StudentId = newRetryAttempt.StudentId,
                    ExerciseId = newRetryAttempt.ExerciseId,
                    AttemptId = newRetryAttempt.AttemptId,
                    GameType = newRetryAttempt.GameType,
                    Difficulty = newRetryAttempt.Difficulty,
                    Status = newRetryAttempt.Status,
                    CorrectAnswer = newRetryAttempt.CorrectAnswer,
                    AttemptNumber = newRetryAttempt.AttemptNumber
                };
            }

            // Step 1: Load the pending attempt (the "generated sentence")
            var pendingAttempt = await _db.GameAttempts
                .Where(a => a.StudentId == request.StudentId && a.ExerciseId == request.ExerciseId && a.Status == AttemptStatus.Pending)
                .FirstOrDefaultAsync(ct);

            if (pendingAttempt == null)
            {
                _logger.LogWarning(
                    "No pending attempt found for StudentId={StudentId}, ExerciseId={ExerciseId}",
                    request.StudentId, request.ExerciseId
                );
                throw new InvalidOperationException("No pending attempt found. Generate a sentence first.");
            }

            // Step 2: Compare answers
            var isCorrect = request.GivenAnswer.SequenceEqual(pendingAttempt.CorrectAnswer);
            var status = isCorrect ? AttemptStatus.Success : AttemptStatus.Failure;

            // Step 3: Calculate attempt number
            var lastAttempt = await _db.GameAttempts
                .Where(a =>
                    a.StudentId == request.StudentId &&
                    a.ExerciseId == request.ExerciseId &&
                    a.Status != AttemptStatus.Pending)
                .OrderByDescending(a => a.AttemptNumber)
                .FirstOrDefaultAsync(ct);

            var nextAttemptNumber = (lastAttempt == null)
                ? 1
                : lastAttempt.AttemptNumber + 1;

            // Step 4: Create new attempt record (not update the pending one)
            var newAttempt = new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                ExerciseId = request.ExerciseId,
                StudentId = request.StudentId,
                GameType = pendingAttempt.GameType,
                Difficulty = pendingAttempt.Difficulty,
                CorrectAnswer = pendingAttempt.CorrectAnswer,
                GivenAnswer = request.GivenAnswer,
                Status = status,
                AttemptNumber = nextAttemptNumber,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.GameAttempts.Add(newAttempt);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Attempt saved. StudentId={StudentId}, AttemptId={AttemptId}, ExerciseId={ExerciseId}, GameType={GameType}, Difficulty={Difficulty}, Status={Status}, AttemptNumber={AttemptNumber}",
                request.StudentId, newAttempt.AttemptId, request.ExerciseId, newAttempt.GameType, newAttempt.Difficulty, status, nextAttemptNumber
            );

            // Step 5: Return result to FE
            return new SubmitAttemptResult
            {
                StudentId = request.StudentId,
                ExerciseId = request.ExerciseId,
                AttemptId = newAttempt.AttemptId,
                GameType = newAttempt.GameType,
                Difficulty = newAttempt.Difficulty,
                Status = status,
                CorrectAnswer = newAttempt.CorrectAnswer,
                AttemptNumber = nextAttemptNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while submitting attempt. StudentId={StudentId}, ExerciseId={ExerciseId}",
                request.StudentId, request.ExerciseId
            );
            throw;
        }
    }

    public async Task<PagedResult<object>> GetHistoryAsync(
    Guid studentId, bool summary, int page, int pageSize, bool getPending, CancellationToken ct)
    {
        try
        {
            if (page < 1)
            {
                _logger.LogWarning("Invalid page {Page}. Resetting to 1.", page);
                page = 1;
            }

            if (pageSize < 1)
            {
                _logger.LogWarning("Invalid pageSize {PageSize}. Resetting to default 10.", pageSize);
                pageSize = 10;
            }

            if (pageSize > 100)
            {
                _logger.LogWarning("PageSize {PageSize} too large. Capping to 100.", pageSize);
                pageSize = 100;
            }

            _logger.LogInformation(
                "Fetching history. StudentId={StudentId}, Summary={Summary}, Page={Page}, PageSize={PageSize}, GetPending={GetPending}",
                studentId, summary, page, pageSize, getPending);

            var attempts = _db.GameAttempts
                .AsNoTracking()
                .Where(a => a.StudentId == studentId);

            if (!getPending)
            {
                attempts = attempts.Where(a => a.Status != AttemptStatus.Pending);
            }

            if (summary)
            {
                var query = attempts
                    .GroupBy(a => new { a.GameType, a.Difficulty })
                    .Select(g => new SummaryHistoryDto
                    {
                        GameType = g.Key.GameType,
                        Difficulty = g.Key.Difficulty,
                        AttemptsCount = g.Count(),
                        TotalSuccesses = g.Count(x => x.Status == AttemptStatus.Success),
                        TotalFailures = g.Count(x => x.Status == AttemptStatus.Failure)
                    });

                var total = await query.CountAsync(ct);
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                _logger.LogInformation(
                    "Summary history retrieved. StudentId={StudentId}, Records={Count}, TotalCount={Total}, GetPending={GetPending}",
                    studentId, items.Count, total, getPending);

                return new PagedResult<object>
                {
                    Items = items,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = total
                };
            }
            else
            {
                var total = await attempts.CountAsync(ct);

                var items = await attempts
                    .OrderByDescending(a => a.CreatedAt)
                    .ThenByDescending(a => a.AttemptId) // тай-брейкер по равным CreatedAt
                    .Select(a => new AttemptHistoryDto
                    {
                        AttemptId = a.AttemptId,
                        GameType = a.GameType,
                        Difficulty = a.Difficulty,
                        GivenAnswer = a.GivenAnswer,
                        CorrectAnswer = a.CorrectAnswer,
                        Status = a.Status,
                        CreatedAt = a.CreatedAt
                    })
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                _logger.LogInformation(
                    "Full history retrieved. StudentId={StudentId}, Records={Count}, TotalCount={Total}, GetPending={GetPending}",
                    studentId, items.Count, total, getPending);

                return new PagedResult<object>
                {
                    Items = items,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = total
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while fetching history. StudentId={StudentId}, Summary={Summary}, GetPending={GetPending}",
                studentId, summary, getPending);

            return new PagedResult<object>
            {
                Items = Array.Empty<object>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0
            };
        }
    }

    public async Task<PagedResult<MistakeDto>> GetMistakesAsync(Guid studentId, int page, int pageSize, CancellationToken ct)
    {
        try
        {
            if (page < 1)
            {
                _logger.LogWarning("Invalid page {Page}. Resetting to 1.", page);
                page = 1;
            }

            if (pageSize < 1)
            {
                _logger.LogWarning("Invalid pageSize {PageSize}. Resetting to default 10.", pageSize);
                pageSize = 10;
            }

            if (pageSize > 100)
            {
                _logger.LogWarning("PageSize {PageSize} too large. Capping to 100.", pageSize);
                pageSize = 100;
            }

            _logger.LogInformation("Fetching mistakes. StudentId={StudentId}, Page={Page}, PageSize={PageSize}", studentId, page, pageSize);

            var attempts = await _db.GameAttempts
                .Where(a => a.StudentId == studentId && a.Status != AttemptStatus.Pending)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync(ct);

            var mistakes = attempts
                .GroupBy(a => a.ExerciseId)
                .Where(g => !g.Any(x => x.Status == AttemptStatus.Success))
                .Select(g => new MistakeDto
                {
                    AttemptId = g.First().AttemptId,
                    GameType = g.First().GameType,
                    Difficulty = g.First().Difficulty,
                    CorrectAnswer = g.First().CorrectAnswer,
                    WrongAnswers = g.Where(x => x.Status == AttemptStatus.Failure).Select(x => x.GivenAnswer).ToList()
                })
                .ToList();

            var total = mistakes.Count;
            var items = mistakes.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            _logger.LogInformation("Mistakes retrieved. StudentId={StudentId}, Records={Count}, TotalCount={Total}", studentId, items.Count, total);

            return new PagedResult<MistakeDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching mistakes. StudentId={StudentId}", studentId);
            return new PagedResult<MistakeDto> { Items = Array.Empty<MistakeDto>(), Page = page, PageSize = pageSize, TotalCount = 0 };
        }
    }

    public async Task<PagedResult<SummaryHistoryWithStudentDto>> GetAllHistoriesAsync(int page, int pageSize, CancellationToken ct)
    {
        try
        {
            if (page < 1)
            {
                _logger.LogWarning("Invalid page {Page}. Resetting to 1.", page);
                page = 1;
            }

            if (pageSize < 1)
            {
                _logger.LogWarning("Invalid pageSize {PageSize}. Resetting to default 10.", pageSize);
                pageSize = 10;
            }

            if (pageSize > 100)
            {
                _logger.LogWarning("PageSize {PageSize} too large. Capping to 100.", pageSize);
                pageSize = 100;
            }

            _logger.LogInformation("Fetching all histories. Page={Page}, PageSize={PageSize}", page, pageSize);

            var query = _db.GameAttempts
                .Where(a => a.Status != AttemptStatus.Pending)
                .GroupBy(a => new { a.StudentId, a.GameType, a.Difficulty })
                .Select(g => new SummaryHistoryWithStudentDto
                {
                    StudentId = g.Key.StudentId,
                    GameType = g.Key.GameType,
                    Difficulty = g.Key.Difficulty,
                    AttemptsCount = g.Count(),
                    TotalSuccesses = g.Count(x => x.Status == AttemptStatus.Success),
                    TotalFailures = g.Count(x => x.Status == AttemptStatus.Failure),
                    StudentFirstName = "",
                    StudentLastName = "",
                    Timestamp = g.Max(x => x.CreatedAt)
                });

            var total = await query.CountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            _logger.LogInformation("All histories retrieved. Records={Count}, TotalCount={Total}", items.Count, total);

            return new PagedResult<SummaryHistoryWithStudentDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all histories from GameService.");
            throw;
        }
    }

    public async Task<AttemptHistoryDto> GetAttemptDetailsAsync(Guid studentId, Guid attemptId, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching attempt details. UserId={UserId}, AttemptId={AttemptId}", studentId, attemptId);

            var attempt = await _db.GameAttempts
                .Where(a => a.StudentId == studentId && a.AttemptId == attemptId)
                .FirstOrDefaultAsync(ct);

            if (attempt == null)
            {
                _logger.LogWarning("Attempt not found. UserId={UserId}, AttemptId={AttemptId}", studentId, attemptId);
                throw new InvalidOperationException($"Attempt {attemptId} not found for user {studentId}");
            }

            var result = new AttemptHistoryDto
            {
                AttemptId = attempt.AttemptId,
                GameType = attempt.GameType,
                Difficulty = attempt.Difficulty,
                GivenAnswer = attempt.GivenAnswer,
                CorrectAnswer = attempt.CorrectAnswer,
                Status = attempt.Status,
                CreatedAt = attempt.CreatedAt
            };

            _logger.LogInformation("Attempt details retrieved. UserId={UserId}, AttemptId={AttemptId}, GameType={GameType}, Status={Status}", studentId, attemptId, result.GameType, result.Status);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching attempt details. UserId={UserId}, AttemptId={AttemptId}", studentId, attemptId);
            throw;
        }
    }

    public async Task<List<AttemptedSentenceResult>> SaveGeneratedSentencesAsync(GeneratedSentenceDto dto, CancellationToken ct)
    {
        try
        {
            var resultList = new List<AttemptedSentenceResult>();

            foreach (var sentence in dto.Sentences)
            {
                var exerciseId = Guid.NewGuid();

                var attempt = new GameAttempt
                {
                    AttemptId = exerciseId, // AttemptId = ExerciseId for the initial save only to keep frontend compatibility, needs refactor later
                    ExerciseId = exerciseId,
                    StudentId = dto.StudentId,
                    GameType = dto.GameType,
                    Difficulty = dto.Difficulty,
                    CorrectAnswer = sentence.CorrectAnswer,
                    GivenAnswer = new(),
                    Status = AttemptStatus.Pending,
                    AttemptNumber = 0,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _db.GameAttempts.Add(attempt);

                resultList.Add(new AttemptedSentenceResult
                {
                    AttemptId = exerciseId, // Return the exerciseId as AttemptId for frontend compatibility, needs refactor later
                    Original = sentence.Original,
                    Words = sentence.CorrectAnswer,
                    Difficulty = dto.Difficulty.ToString().ToLowerInvariant(),
                    Nikud = sentence.Nikud
                });
            }

            await _db.SaveChangesAsync(ct);

            return resultList;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SaveGeneratedSentencesAsync was canceled. StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}", dto.StudentId, dto.GameType, dto.Difficulty);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while saving generated sentences. StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}", dto.StudentId, dto.GameType, dto.Difficulty);
            throw;
        }
    }

    public async Task DeleteAllGamesHistoryAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Deleting all game history...");
            var deletedAttempts = await _db.GameAttempts.ExecuteDeleteAsync(ct);
            _logger.LogInformation("Deleted {Count} game attempts", deletedAttempts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting all game history.");
            throw;
        }
    }
}