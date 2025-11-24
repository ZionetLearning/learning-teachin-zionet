using Manager.Models.Users;

namespace Manager.Services.Clients.Accessor.Models.Users;

public sealed record UpdateUserLanguageAccessorRequest
{
    public required Guid UserId { get; init; }
    public required SupportedLanguage Language { get; init; }
}
