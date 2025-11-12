namespace Manager.Models.Words;

public class WordExplainRequest
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string Word { get; set; }
    public required string Context { get; set; }
}
