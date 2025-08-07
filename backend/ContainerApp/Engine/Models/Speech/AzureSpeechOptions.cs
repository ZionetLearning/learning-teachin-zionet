using System.ComponentModel.DataAnnotations;

namespace Engine.Models.Speech;

public record AzureSpeechOptions
{
    public const string SectionName = "AzureSpeech";

    [Required]
    public string SubscriptionKey { get; set; } = string.Empty;

    [Required]
    public string Region { get; set; } = string.Empty;

    public string DefaultVoice { get; set; } = "he-IL-HilaNeural";
    public int TimeoutSeconds { get; set; } = 30;
}
