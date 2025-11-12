namespace Manager.Models.Sentences;

public sealed class SplitSentenceResponse
{
    public List<SplitSentenceItem> Sentences { get; init; } = new();
}

public sealed class SplitSentenceItem
{
    public List<string> Words { get; init; } = new();
    public string Text { get; init; } = string.Empty;
    public string Difficulty { get; init; } = string.Empty;
    public bool Nikud { get; init; }
}
