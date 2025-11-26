
namespace Manager.Models.Users;

public sealed record UserLanguage
{
    public required Guid UserId { get; init; }
    public required SupportedLanguage Language { get; init; }
}
