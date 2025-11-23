namespace Manager.Models.Users;

public sealed record UpdateUserLanguageRequest
{
    public required SupportedLanguage PreferredLanguage { get; init; }
}
