namespace Manager.Models.Words;

public sealed record WordExplainEngineRequest
{
    public Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string Word { get; init; }
    public required string Context { get; init; }
}
