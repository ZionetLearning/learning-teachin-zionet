using Manager.Models.Sentences;

public sealed class SplitSentenceResponse
{
    public string SentenceId { get; init; } = string.Empty;
    public SplitSentenceSplit Split { get; init; } = new();
}

public sealed class SplitSentenceSplit
{
    public List<SplitSentenceItem> Sentences { get; init; } = new();
}
