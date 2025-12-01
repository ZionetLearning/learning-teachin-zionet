namespace Manager.Models.Summaries;

public sealed record GetGamePracticeSummaryResponse
{
    public required GamePracticeSummary Summary { get; init; }
    public List<GameTypeStats> ByGameType { get; init; } = new();
    public List<DailyGameStats> Daily { get; init; } = new();
    public required MistakesData Mistakes { get; init; }
}

public sealed record GamePracticeSummary
{
    public int TotalAttempts { get; init; }
    public decimal SuccessRate { get; init; }
    public decimal AverageAccuracy { get; init; }
}

public sealed record GameTypeStats
{
    public required string GameType { get; init; }
    public int Attempts { get; init; }
    public decimal SuccessRate { get; init; }
    public decimal AverageAccuracy { get; init; }
    public int MistakesCount { get; init; }
}

public sealed record DailyGameStats
{
    public required DateTime Date { get; init; }
    public int Attempts { get; init; }
    public decimal SuccessRate { get; init; }
}

/// <summary>
/// Mistakes data - contains only uncorrected mistakes (exercises not yet answered correctly)
/// </summary>
public sealed record MistakesData
{
    public required MistakesSummary Summary { get; init; }
    public List<MistakePattern> Patterns { get; init; } = new();
    public List<MistakeExample> UncorrectedExamples { get; init; } = new();
}

public sealed record MistakesSummary
{
    /// <summary>
    /// Total number of mistakes from uncorrected exercises only
    /// </summary>
    public int TotalUncorrectedMistakes { get; init; }

    /// <summary>
    /// Number of exercises with multiple attempts that are still uncorrected
    /// </summary>
    public int RetriedButNotCorrected { get; init; }

    /// <summary>
    /// Number of distinct exercises with mistakes that haven't been corrected yet
    /// </summary>
    public int UncorrectedExercises { get; init; }
}

public sealed record MistakePattern
{
    public required string GameType { get; init; }
    public required string Difficulty { get; init; }
    public int Count { get; init; }
}

public sealed record MistakeExample
{
    public required Guid ExerciseId { get; init; }
    public required string GameType { get; init; }
    public required string Difficulty { get; init; }
    public List<string> CorrectAnswer { get; init; } = new();
    public List<string> GivenAnswer { get; init; } = new();
    public decimal Accuracy { get; init; }
    public int AttemptNumber { get; init; }
    public required DateTime CreatedAt { get; init; }
}
