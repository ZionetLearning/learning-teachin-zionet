namespace IntegrationTests.Models.Ai.Sentences;

public class SentenceGenerationResponse
{
    public required string RequestId { get; set; }
    public required List<GeneratedSentenceResultItem> Sentences { get; set; }
}

public class GeneratedSentenceResultItem
{
    public required Guid ExerciseId { get; set; }
    public required string Text { get; set; }
  public required List<string> Words { get; set; }
    public required string Difficulty { get; set; }
    public required bool Nikud { get; set; }
}
