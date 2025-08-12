namespace Manager.Models.Speech;

public record SpeechMetadata
{
    public int AudioLength { get; set; }
    public string AudioFormat { get; set; } = "wav";
    public TimeSpan ProcessingDuration { get; set; }
}