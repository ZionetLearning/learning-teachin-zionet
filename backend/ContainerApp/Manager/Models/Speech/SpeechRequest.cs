namespace Manager.Models.Speech;

public record SpeechRequest
{
    public string Text { get; set; } = "";

    public string VoiceName { get; set; } = "he-IL-HilaNeural";
}