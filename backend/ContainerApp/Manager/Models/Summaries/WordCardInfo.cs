namespace Manager.Models.Summaries;

public sealed record WordCardInfo
{
    public required Guid CardId { get; init; }
    public required string Hebrew { get; init; }
    public required string English { get; init; }
    public required DateTime Timestamp { get; init; }
}
