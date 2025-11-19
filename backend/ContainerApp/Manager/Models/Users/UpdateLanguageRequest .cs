namespace Manager.Models.Users;

public class UpdateLanguageRequest
{
    public required SupportedLanguage PreferredLanguage { get; set; }
}
