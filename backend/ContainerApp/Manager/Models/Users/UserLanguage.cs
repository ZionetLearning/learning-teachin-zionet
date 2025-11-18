
namespace Manager.Models.Users;

public class UserLanguage
{
    public required Guid UserId { get; set; }
    public required SupportedLanguage Language { get; set; }
}
