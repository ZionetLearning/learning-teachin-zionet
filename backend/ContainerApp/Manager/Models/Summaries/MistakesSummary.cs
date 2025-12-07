namespace Manager.Models.Summaries;

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
