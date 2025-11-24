namespace Manager.Models.Words;

public class WordExplainResponse
{
    public required Guid Id { get; set; }
    public required string Definition { get; set; }
    public required string Explanation { get; set; }
}