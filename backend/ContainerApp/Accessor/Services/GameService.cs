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
            _logger.LogInformation("Submitting attempt. StudentId={StudentId}, AttemptId={AttemptId}, GivenAnswer={GivenAnswer}", request.StudentId, request.AttemptId, string.Join(" ", request.GivenAnswer ?? new()));

            if (request.GivenAnswer is null)
            {
                throw new ArgumentException("GivenAnswer must not be null.");
            }

            // Step 1: Load the pending attempt (the "generated sentence")
            var pendingAttempt = await _db.GameAttempts
                .Where(a => a.StudentId == request.StudentId && a.AttemptId == request.AttemptId)
                .FirstOrDefaultAsync(ct);

            if (pendingAttempt == null)
            {
                _logger.LogWarning(
                    "No pending attempt found for StudentId={StudentId}, AttemptId={AttemptId}",
                    request.StudentId, request.AttemptId
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
                    a.GameType == pendingAttempt.GameType &&
                    a.Difficulty == pendingAttempt.Difficulty &&
                    a.CorrectAnswer.SequenceEqual(pendingAttempt.CorrectAnswer) &&
                    a.Status != AttemptStatus.Pending)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(ct);

            var nextAttemptNumber = (lastAttempt == null || lastAttempt.Status == AttemptStatus.Success)
                ? 1
                : lastAttempt.AttemptNumber + 1;

            // Step 4: Save new attempt row
            var attempt = new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                StudentId = request.StudentId,
                GameType = pendingAttempt.GameType,
                Difficulty = pendingAttempt.Difficulty,
                CorrectAnswer = pendingAttempt.CorrectAnswer,
                GivenAnswer = request.GivenAnswer ?? new(),
                Status = status,
                AttemptNumber = nextAttemptNumber,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.GameAttempts.Add(attempt);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Attempt saved. StudentId={StudentId}, AttemptId={AttemptId}, GameType={GameType}, Difficulty={Difficulty}, Status={Status}, AttemptNumber={AttemptNumber}",
                request.StudentId, attempt.AttemptId, attempt.GameType, attempt.Difficulty, status, nextAttemptNumber
            );

            // Step 5: Return result to FE
            return new SubmitAttemptResult
            {
                StudentId = request.StudentId,
                GameType = attempt.GameType,
                Difficulty = attempt.Difficulty,
                Status = status,
                CorrectAnswer = attempt.CorrectAnswer,
                AttemptNumber = nextAttemptNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while submitting attempt. StudentId={StudentId}, AttemptId={AttemptId}",
                request.StudentId, request.AttemptId
            );
            throw;
        }
    }

    public async Task<PagedResult<object>> GetHistoryAsync(Guid studentId, bool summary, int page, int pageSize, CancellationToken ct)
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

            _logger.LogInformation("Fetching history. StudentId={StudentId}, Summary={Summary}, Page={Page}, PageSize={PageSize}", studentId, summary, page, pageSize);

            if (summary)
            {
                var query = _db.GameAttempts
                    .Where(a => a.StudentId == studentId && a.Status != AttemptStatus.Pending)
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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

                _logger.LogInformation("Summary history retrieved. StudentId={StudentId}, Records={Count}, TotalCount={Total}", studentId, items.Count, total);

                return new PagedResult<object> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
            }
            else
            {
                var query = _db.GameAttempts
                    .Where(a => a.StudentId == studentId)
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
                    .OrderByDescending(a => a.CreatedAt);

                var total = await query.CountAsync(ct);
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

                _logger.LogInformation("Full history retrieved. StudentId={StudentId}, Records={Count}, TotalCount={Total}", studentId, items.Count, total);

                return new PagedResult<object> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching history. StudentId={StudentId}, Summary={Summary}", studentId, summary);
            return new PagedResult<object> { Items = Array.Empty<object>(), Page = page, PageSize = pageSize, TotalCount = 0 };
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
                .GroupBy(a => new { a.GameType, a.Difficulty, CorrectKey = string.Join(" ", a.CorrectAnswer) })
                .Where(g => !g.Any(x => x.Status == AttemptStatus.Success))
                .Select(g => new MistakeDto
                {
                    GameType = g.Key.GameType,
                    Difficulty = g.Key.Difficulty,
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
                    TotalFailures = g.Count(x => x.Status == AttemptStatus.Failure)
                });

            var total = await query.CountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            _logger.LogInformation("All histories retrieved. Records={Count}, TotalCount={Total}", items.Count, total);

            return new PagedResult<SummaryHistoryWithStudentDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all histories.");
            return new PagedResult<SummaryHistoryWithStudentDto> { Items = Array.Empty<SummaryHistoryWithStudentDto>(), Page = page, PageSize = pageSize, TotalCount = 0 };
        }
    }

    public async Task<Guid> SaveGeneratedSentenceAsync(GeneratedSentenceDto dto, CancellationToken ct)
    {
        try
        {
            var attempt = new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                StudentId = dto.StudentId,
                GameType = dto.GameType,
                Difficulty = dto.Difficulty,
                CorrectAnswer = dto.CorrectAnswer,
                GivenAnswer = new(),   // student hasn’t answered yet
                Status = AttemptStatus.Pending,
                AttemptNumber = 0,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.GameAttempts.Add(attempt);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Generated sentence saved successfully. AttemptId={AttemptId}, StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}", attempt.AttemptId, dto.StudentId, dto.GameType, dto.Difficulty);

            return attempt.AttemptId;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SaveGeneratedSentenceAsync was canceled. StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}", dto.StudentId, dto.GameType, dto.Difficulty);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while saving generated sentence. StudentId={StudentId}, GameType={GameType}, Difficulty={Difficulty}", dto.StudentId, dto.GameType, dto.Difficulty);
            throw;
        }
    }
}