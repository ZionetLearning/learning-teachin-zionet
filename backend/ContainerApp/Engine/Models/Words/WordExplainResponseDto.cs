namespace Engine.Models.Words;

public class WordExplainResponseDto
{
    public required Guid Id { get; set; }
    public required string Definition { get; set; }
    public required string Explanation { get; set; }
}