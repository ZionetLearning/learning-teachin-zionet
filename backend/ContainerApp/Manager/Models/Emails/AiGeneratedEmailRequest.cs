using Manager.Models.Users;

namespace Manager.Models.Emails;

public class AiGeneratedEmailRequest
{
    public required string Subject { get; set; }
    public required string Purpose { get; set; }
    public SupportedLanguage PreferredLanguageCode { get; init; } = SupportedLanguage.en;

}