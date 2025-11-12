namespace Engine.Models.Sentences;

public sealed record SentencesResponse
{
    public required string RequestId { get; init; }
    public required List<SentenceItem> Sentences { get; init; }
}