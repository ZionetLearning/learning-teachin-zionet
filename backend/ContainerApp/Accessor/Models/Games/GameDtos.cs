using Accessor.Models.GameConfiguration;

namespace Accessor.Models.Games;

/// <summary>
/// Internal DTOs used by service layer (not API layer)
/// Similar to ClassDto pattern - these are returned by services and mapped to Response DTOs by endpoints
/// </summary>

#region History DTOs

public record AttemptHistoryDto
{
    public required Guid ExerciseId { get; init; }
    public required Guid AttemptId { get; init; }
    public required GameName GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required List<string> GivenAnswer { get; init; }
    public required List<string> CorrectAnswer { get; init; }
    public required AttemptStatus Status { get; init; }
    public required decimal Accuracy { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public record SummaryHistoryDto
{
    public required GameName GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required int AttemptsCount { get; init; }
    public required int TotalSuccesses { get; init; }
    public required int TotalFailures { get; init; }
}

public record SummaryHistoryWithStudentDto
{
    public required Guid StudentId { get; init; }
    public required GameName GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required int AttemptsCount { get; init; }
    public required int TotalSuccesses { get; init; }
    public required int TotalFailures { get; init; }
    public required string StudentFirstName { get; init; }
    public required string StudentLastName { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}

#endregion

#region Mistake DTOs

public record MistakeDto
{
    public required Guid ExerciseId { get; init; }
    public required Guid AttemptId { get; init; }
    public required GameName GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required List<string> CorrectAnswer { get; init; }
    public required List<MistakeAttemptDto> Mistakes { get; init; }
}

public record MistakeAttemptDto
{
    public required Guid AttemptId { get; init; }
    public required List<string> WrongAnswer { get; init; }
    public required decimal Accuracy { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

#endregion

#region Generated Sentence DTOs

public record GeneratedSentenceResultDto
{
    public required Guid ExerciseId { get; init; }
    public required string Text { get; init; }
    public required List<string> Words { get; init; }
    public required string Difficulty { get; init; }
    public required bool Nikud { get; init; }
}

#endregion

#region Paged Result

/// <summary>
/// Generic paged result for internal service layer
/// </summary>
public record PagedResult<T>
{
    public required IEnumerable<T> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public bool HasNextPage => Page * PageSize < TotalCount;
}

#endregion

#region History Wrapper

/// <summary>
/// Wrapper for game history that can contain either summary or detailed data
/// Used internally by service layer
/// </summary>
public record GameHistoryDto
{
    public PagedResult<SummaryHistoryDto>? Summary { get; init; }
    public PagedResult<AttemptHistoryDto>? Detailed { get; init; }

    public bool IsSummary => Summary is not null;
    public bool IsDetailed => Detailed is not null;
}

#endregion

