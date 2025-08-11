using System.ComponentModel.DataAnnotations;

namespace Manager.Models.Speech;

public record SpeechRequest
{
    [Required(ErrorMessage = "Text is required.")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Text must be between 1 and 1000 characters.")]
    public string Text { get; set; } = null!;

    [StringLength(50, ErrorMessage = "VoiceName cannot exceed 50 characters.")]
    public string VoiceName { get; set; } = "he-IL-HilaNeural";
}