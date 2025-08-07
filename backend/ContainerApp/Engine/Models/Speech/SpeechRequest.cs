namespace Engine.Models.Speech;

public record SpeechRequest
{
    public string Text { get; set; } = string.Empty;

    public string VoiceName { get; set; } = "he-IL-HilaNeural";
}