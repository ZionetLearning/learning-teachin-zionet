namespace Engine.Models.Speech;

public record SpeechRequestDto
{
    public string Text { get; set; } = string.Empty;

    public string VoiceName { get; set; } = "he-IL-HilaNeural";
}