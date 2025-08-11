namespace Engine.Models.Speech;

public record SpeechResponse
{
    public string AudioData { get; set; } = string.Empty;
    public List<VisemeData> Visemes { get; set; } = new();
    public SpeechMetadata Metadata { get; set; } = new();
}
