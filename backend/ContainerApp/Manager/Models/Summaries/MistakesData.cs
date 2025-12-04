namespace Manager.Models.Summaries;

/// <summary>
/// Mistakes data - contains only uncorrected mistakes (exercises not yet answered correctly)
/// </summary>
public sealed record MistakesData
{
    public required MistakesSummary Summary { get; init; }
    public List<MistakePattern> Patterns { get; init; } = new();
    public List<MistakeExample> UncorrectedExamples { get; init; } = new();
}
