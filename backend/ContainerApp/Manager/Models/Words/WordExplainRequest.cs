namespace Manager.Models.Words;

public sealed record WordExplainRequest
{
    public required string Word { get; init; }
    public required string Context { get; init; }
}
