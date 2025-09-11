
namespace Accessor.Models.Users;

public class UpdateUserModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public SupportedLanguage? PreferredLanguageCode { get; set; }
    public HebrewLevel? HebrewLevelValue { get; set; }
    public Role? Role { get; set; }
}
