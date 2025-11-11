namespace Manager.Models.Sentences;

public sealed class SentenceResponse
{
    public required string RequestId { get; set; }
    public List<SentenceItem> Sentences { get; init; } = new();
}

public sealed class SentenceItem
{
    public string GameType { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string Difficulty { get; init; } = string.Empty;
    public bool Nikud { get; init; }
}
