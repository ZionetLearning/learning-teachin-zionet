namespace Manager.Models.Speech;

public record SpeechEngineResponse
{
    public string AudioData { get; set; } = string.Empty;
    public List<VisemeData> Visemes { get; set; } = new();
    public SpeechMetadata Metadata { get; set; } = new();
}