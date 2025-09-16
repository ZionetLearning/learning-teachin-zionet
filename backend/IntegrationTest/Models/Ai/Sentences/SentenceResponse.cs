namespace Models.Ai.Sentences;

public sealed class SentenceResponse
{
    public List<SentenceItem> Sentences { get; init; } = new();
}

public sealed class SentenceItem
{
    public string Text { get; init; } = string.Empty;
    public string Difficulty { get; init; } = string.Empty;
    public bool Nikud { get; init; }
}
