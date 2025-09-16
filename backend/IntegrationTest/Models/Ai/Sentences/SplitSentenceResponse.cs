namespace Models.Ai.Sentences;

public sealed class SplitSentenceResponse
{
    public List<SplitSentenceItem> Sentences { get; init; } = new();
}

public sealed class SplitSentenceItem
{
    public List<string> Words { get; init; } = new();
    public string Original { get; init; } = string.Empty;
    public string Difficulty { get; init; } = string.Empty;
    public bool Nikud { get; init; }
}
