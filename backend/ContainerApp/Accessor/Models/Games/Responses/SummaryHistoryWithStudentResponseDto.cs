using Accessor.Models.GameConfiguration;

namespace Accessor.Models.Games.Responses;

/// <summary>
/// DTO representing summary history with student info in response
/// </summary>
public sealed record SummaryHistoryWithStudentResponseDto
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

