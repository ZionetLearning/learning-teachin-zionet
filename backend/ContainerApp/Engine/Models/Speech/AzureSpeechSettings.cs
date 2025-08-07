namespace Engine.Models.Speech;

public record AzureSpeechSettings
{
    public const string SectionName = "AzureSpeech";

    public required string SubscriptionKey { get; set; } = string.Empty;

    public required string Region { get; set; } = string.Empty;

    public string DefaultVoice { get; set; } = "he-IL-HilaNeural";
    public int TimeoutSeconds { get; set; } = 30;
}
