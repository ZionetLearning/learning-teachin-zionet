namespace Manager.Models.Words;

public class WordExplainRequestDto
{
    public required string Word { get; set; }
    public required string Context { get; set; }
}
