namespace Engine.Models.Speech;

public record SpeechMetadata
{
    public int AudioLength { get; set; }
    public string AudioFormat { get; set; } = "wav";
    public DateTime ProcessedAt { get; set; }
    public TimeSpan ProcessingDuration { get; set; }
}
