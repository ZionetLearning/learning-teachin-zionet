using Accessor.DB;
using Accessor.Helpers;
using Accessor.Models.Games;
using Accessor.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class GameService : IGameService
{
    private readonly AccessorDbContext _db;
    private readonly ILogger<GameService> _logger;
    private readonly IMapper _mapper;

    public GameService(AccessorDbContext db, ILogger<GameService> logger, IMapper mapper)
    {
        _db = db;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<SubmitAttemptResult> SubmitAttemptAsync(SubmitAttemptRequest request, CancellationToken ct)
    {
        if (request.StudentId == Guid.Empty)
        {
            throw new InvalidOperationException("StudentId must not be empty.");
        }

        if (request.GivenAnswer is null)
        {
            throw new InvalidOperationException("GivenAnswer must not be null.");
        }

        try
        {
            _logger.LogInformation("Submit attempt requested. ExerciseId={ExerciseId}, StudentId={StudentId}", request.ExerciseId, request.StudentId);

            var original = await _db.GameAttempts
                .FirstOrDefaultAsync(a => a.StudentId == request.StudentId &&
                a.ExerciseId == request.ExerciseId &&
                a.AttemptId == request.ExerciseId, ct); // the original attempt has AttemptId == ExerciseId (exercise without answer)

            if (original is null)
            {
                _logger.LogWarning("Exercise not found. ExerciseId={ExerciseId}", request.ExerciseId);
                throw new KeyNotFoundException("Original exercise not found.");
            }

            var isCorrect = request.GivenAnswer.SequenceEqual(original.CorrectAnswer);

            // If correct, accuracy is 100%, otherwise calculate accuracy
            var accuracy = isCorrect ? 100m : AccuracyCalculator.Calculate(
                original.GameType,
                original.CorrectAnswer,
                request.GivenAnswer
            );

            var last = await _db.GameAttempts
                .Where(a => a.StudentId == original.StudentId && a.ExerciseId == original.ExerciseId)
                .OrderByDescending(a => a.AttemptNumber)
                .FirstOrDefaultAsync(ct);

            var nextNumber = (last?.AttemptNumber ?? 0) + 1;

            var newAttempt = new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                ExerciseId = original.ExerciseId,
                StudentId = original.StudentId,
                GameType = original.GameType,
                Difficulty = original.Difficulty,
                CorrectAnswer = original.CorrectAnswer,
                GivenAnswer = request.GivenAnswer,
                Status = isCorrect ? AttemptStatus.Success : AttemptStatus.Failure,
                AttemptNumber = nextNumber,
                Accuracy = accuracy,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.GameAttempts.Add(newAttempt);

            // If the answer is correct, update the original attempt (first attempt) to Success
            if (isCorrect)
            {
                original.Status = AttemptStatus.Success;
                _db.GameAttempts.Update(original);
                _logger.LogInformation("Original attempt updated to Success. ExerciseId={ExerciseId}", original.ExerciseId);
            }

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("New attempt saved. AttemptId={NewId}, ExerciseId={ExerciseId}, Number={Number}, Status={Status}, Accuracy={Accuracy}%",
                newAttempt.AttemptId, original.ExerciseId, nextNumber, newAttempt.Status, newAttempt.Accuracy);

            return _mapper.Map<SubmitAttemptResult>(newAttempt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while submitting attempt. ExerciseId={ExerciseId}, StudentId={StudentId}", request.ExerciseId, request.StudentId);
            throw;
        }
    }

    public async Task<GameHistoryResponse> GetHistoryAsync(
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

                return new GameHistoryResponse
                {
                    Summary = new PagedResult<SummaryHistoryDto>
                    {
                        Items = items,
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = total
                    }
                };
            }
            else
            {
                var total = await attempts.CountAsync(ct);

                var items = await attempts
                    .OrderByDescending(a => a.CreatedAt)
                    .ThenByDescending(a => a.AttemptId)
                    .Select(a => new AttemptHistoryDto
                    {
                        ExerciseId = a.ExerciseId,
                        AttemptId = a.AttemptId,
                        GameType = a.GameType,
                        Difficulty = a.Difficulty,
                        GivenAnswer = a.GivenAnswer,
                        CorrectAnswer = a.CorrectAnswer,
                        Status = a.Status,
                        Accuracy = a.Accuracy,
                        CreatedAt = a.CreatedAt
                    })
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                _logger.LogInformation(
                    "Full history retrieved. StudentId={StudentId}, Records={Count}, TotalCount={Total}, GetPending={GetPending}",
                    studentId, items.Count, total, getPending);

                return new GameHistoryResponse
                {
                    Detailed = new PagedResult<AttemptHistoryDto>
                    {
                        Items = items,
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = total
                    }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while fetching history. StudentId={StudentId}, Summary={Summary}, GetPending={GetPending}",
                studentId, summary, getPending);

            return summary
                ? new GameHistoryResponse
                {
                    Summary = new PagedResult<SummaryHistoryDto>
                    {
                        Items = Array.Empty<SummaryHistoryDto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0
                    }
                }
                : new GameHistoryResponse
                {
                    Detailed = new PagedResult<AttemptHistoryDto>
                    {
                        Items = Array.Empty<AttemptHistoryDto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0
                    }
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

            // Get all attempts, group by exercise, and filter for exercises with only failures
            var allAttempts = await _db.GameAttempts
                .AsNoTracking()
                .Where(a => a.StudentId == studentId && a.Status != AttemptStatus.Pending)
                .ToListAsync(ct);

            var mistakes = allAttempts
                .GroupBy(a => a.ExerciseId)
                .Where(g => !g.Any(a => a.Status == AttemptStatus.Success))
                .Select(g => new MistakeDto
                {
                    ExerciseId = g.Key,
                    GameType = g.First().GameType,
                    Difficulty = g.First().Difficulty,
                    CorrectAnswer = g.First().CorrectAnswer,
                    Mistakes = g.Where(a => a.Status == AttemptStatus.Failure)
                                .OrderByDescending(a => a.CreatedAt)
                                .Select(a => new MistakeAttemptDto
                                {
                                    AttemptId = a.AttemptId,
                                    WrongAnswer = a.GivenAnswer,
                                    Accuracy = a.Accuracy,
                                    CreatedAt = a.CreatedAt
                                }).ToList()
                })
                .OrderByDescending(m => m.Mistakes.Max(a => a.CreatedAt))
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
                ExerciseId = attempt.ExerciseId,
                AttemptId = attempt.AttemptId,
                GameType = attempt.GameType,
                Difficulty = attempt.Difficulty,
                GivenAnswer = attempt.GivenAnswer,
                CorrectAnswer = attempt.CorrectAnswer,
                Status = attempt.Status,
                Accuracy = attempt.Accuracy,
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
                    AttemptId = exerciseId,
                    ExerciseId = exerciseId,
                    StudentId = dto.StudentId,
                    GameType = dto.GameType,
                    Difficulty = dto.Difficulty,
                    CorrectAnswer = sentence.CorrectAnswer,
                    GivenAnswer = new(),
                    Status = AttemptStatus.Pending,
                    AttemptNumber = 0,
                    Accuracy = 0m,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _db.GameAttempts.Add(attempt);

                resultList.Add(new AttemptedSentenceResult
                {
                    AttemptId = exerciseId,
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